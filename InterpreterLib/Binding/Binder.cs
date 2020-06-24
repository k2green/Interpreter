using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using InterpreterLib.Binding.Tree;
using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Binding {
	internal sealed class Binder : GLangBaseVisitor<BoundExpression> {

		private BoundScope scope;
		private DiagnosticContainer diagnostics;

		private Binder(BoundScope parent) {
			scope = new BoundScope(parent);
			diagnostics = new DiagnosticContainer();
		}

		public static DiagnosticResult<BoundGlobalScope> BindGlobalScope(BoundGlobalScope previous, IParseTree tree) {
			Binder binder = new Binder(CreateParentScopes(previous));

			BoundExpression root = binder.Visit(tree);

			BoundGlobalScope glob = new BoundGlobalScope(previous, binder.diagnostics, binder.scope.GetVariables(), root);
			return new DiagnosticResult<BoundGlobalScope>(binder.diagnostics, glob);
		}

		private static BoundScope CreateParentScopes(BoundGlobalScope previous) {
			if (previous == null)
				return null;

			var stack = new Stack<BoundGlobalScope>();
			while (previous != null) {
				stack.Push(previous);
				previous = previous.Previous;
			}

			BoundScope parent = null;

			while (stack.Count > 0) {
				var scope = new BoundScope(parent);
				previous = stack.Pop();

				foreach (BoundVariable variable in previous.Variables)
					scope.TryDefine(variable);

				parent = scope;
			}

			return parent;
		}

		public override BoundExpression VisitLiteral([NotNull] GLangParser.LiteralContext context) {
			if (context.INTEGER() != null)
				return new BoundLiteral(int.Parse(context.INTEGER().GetText()));

			if (context.BOOLEAN() != null)
				return new BoundLiteral(bool.Parse(context.BOOLEAN().GetText()));

			if (context.IDENTIFIER() != null) {
				if (scope.TryLookup(context.IDENTIFIER().GetText(), out var variable)) {
					return new BoundVariableExpression(variable);
				} else {
					diagnostics.AddDiagnostic(Diagnostic.ReportUndefinedVariable(context.Start));
				}
			}

			diagnostics.AddDiagnostic(Diagnostic.ReportInvalidSyntax(context.Start, context.GetText()));
			return null;
		}

		public override BoundExpression VisitUnaryExpression([NotNull] GLangParser.UnaryExpressionContext context) {
			// Return the binding of the base case
			if (context.atom != null)
				return Visit(context.atom);

			// Return the result of the expression binding in the case of inputs '(' expression ')'
			if (context.binaryExpression() != null)
				return Visit(context.binaryExpression());

			// If we have the case where a unary operator preceds a unary expression
			if (context.op != null && context.unaryExpression() != null) {
				// Bind the 
				var operand = Visit(context.unaryExpression());

				if (operand == null)
					return null;

				var op = UnaryOperator.Bind(context.op.Text, operand.ValueType);

				if (op == null) {
					diagnostics.AddDiagnostic(Diagnostic.ReportInvalidUnaryOperator(context.op, operand.ValueType));
					return null;
				}

				return new BoundUnaryExpression(op, operand);
			}

			diagnostics.AddDiagnostic(Diagnostic.ReportInvalidSyntax(context.Start, context.GetText()));
			return null;
		}

		public override BoundExpression VisitBinaryExpression([NotNull] GLangParser.BinaryExpressionContext context) {
			// Return the binding of the base case
			if (context.atom != null)
				return Visit(context.atom);

			// Handles expressions with sub-expressions
			if (context.left != null && context.op != null && context.right != null) {
				var left = Visit(context.left);
				var right = Visit(context.right);

				// Return null if the binding of either child fails
				if (left == null || right == null)
					return null;

				// Type checking for operators
				var op = BinaryOperator.Bind(context.op.Text, left.ValueType, right.ValueType);

				if (op == null) {
					// Report the operator is invalid for the given types
					diagnostics.AddDiagnostic(Diagnostic.ReportInvalidBinaryOperator(context.op, left.ValueType, right.ValueType));
					return null;
				}

				// Return a new bound binary expression.
				return new BoundBinaryExpression(left, op, right);
			}

			diagnostics.AddDiagnostic(Diagnostic.ReportInvalidSyntax(context.Start, context.GetText()));
			return null;
		}

		public override BoundExpression VisitAssignmentExpression([NotNull] GLangParser.AssignmentExpressionContext context) {
			bool isDeclaration = context.var != null && context.var.Text.Equals("var");

			// Report invalid assignment expression if any of the child nodes are null
			if (context.IDENTIFIER() == null || context.ASSIGNMENT_OPERATOR() == null || context.binaryExpression() == null) {
				diagnostics.AddDiagnostic(Diagnostic.ReportInvalidSyntax(context.Start, context.GetText()));
				return null;
			}

			var operandExpression = Visit(context.binaryExpression());

			// Return null as error should have already been reported
			if (operandExpression == null)
				return null;

			BoundVariable variable = new BoundVariable(context.IDENTIFIER().GetText(), false, operandExpression.ValueType);

			if (isDeclaration) {
				if (!scope.TryDefine(variable)) {
					diagnostics.AddDiagnostic(Diagnostic.ReportRedefineVariable(context.Start));
					return null;
				}
			} else {
				if(!scope.TryLookup(variable.Name, out var _)) {
					diagnostics.AddDiagnostic(Diagnostic.ReportUndefinedVariable(context.Start));
					return null;
				}
			}

			// Bind the assignment operation
			return new BoundAssignmentExpression(variable, operandExpression);
		}

		public override BoundExpression VisitStatement([NotNull] GLangParser.StatementContext context) {
			if (context.assignmentExpression() != null)
				return Visit(context.assignmentExpression());

			if (context.binaryExpression() != null)
				return Visit(context.binaryExpression());

			diagnostics.AddDiagnostic(Diagnostic.ReportInvalidStatement(context.Start, context.GetText()));
			return null;
		}
	}
}
