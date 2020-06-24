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

			var token = context.Start;

			if (context.IDENTIFIER() != null) {
				if (scope.TryLookup(context.IDENTIFIER().GetText(), out var variable)) {
					return new BoundVariableExpression(variable);
				} else {
					diagnostics.AddDiagnostic(Diagnostic.ReportUndefinedVariable(token.Line, token.Column, context.IDENTIFIER().GetText()));
				}
			}

			diagnostics.AddDiagnostic(Diagnostic.ReportInvalidSyntax(token.Line, token.Column, context.GetText()));
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
					diagnostics.AddDiagnostic(Diagnostic.ReportInvalidUnaryOperator(context.op.Line, context.op.Column, context.op.Text, operand.ValueType));
					return null;
				}

				return new BoundUnaryExpression(op, operand);
			}

			diagnostics.AddDiagnostic(Diagnostic.ReportInvalidSyntax(context.Start.Line, context.Start.Column, $"Unary Expression {context.GetText()}"));
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
					diagnostics.AddDiagnostic(Diagnostic.ReportInvalidBinaryOperator(context.op.Line, context.op.Column, context.op.Text, left.ValueType, right.ValueType));
					return null;
				}

				// Return a new bound binary expression.
				return new BoundBinaryExpression(left, op, right);
			}

			diagnostics.AddDiagnostic(Diagnostic.ReportInvalidSyntax(context.Start.Line, context.Start.Column, $"Binary Expression {context.GetText()}"));
			return null;
		}

		public override BoundExpression VisitAssignmentExpression([NotNull] GLangParser.AssignmentExpressionContext context) {
			bool isReadOnly = false;
			bool isDeclaration = false;

			if (context.decl != null) {
				isReadOnly = context.decl.Text.Equals("val");
				isDeclaration = isReadOnly || context.decl.Text.Equals("var");
			}

			// Report invalid assignment expression if any of the child nodes are null
			if (context.IDENTIFIER() == null || context.ASSIGNMENT_OPERATOR() == null || context.binaryExpression() == null) {
				diagnostics.AddDiagnostic(Diagnostic.ReportInvalidSyntax(context.Start.Line, context.Start.Column, $"Assignment Expression {context.GetText()}"));
				return null;
			}

			var operandExpression = Visit(context.binaryExpression());

			// Return null as error should have already been reported
			if (operandExpression == null)
				return null;

			BoundVariable variable = new BoundVariable(context.IDENTIFIER().GetText(), isReadOnly, operandExpression.ValueType);
			var token = context.Start;

			if (isDeclaration) {
				if (!scope.TryDefine(variable)) {
					diagnostics.AddDiagnostic(Diagnostic.ReportRedefineVariable(token.Line, token.Column, scope[variable.Name], variable));
					return null;
				}
			}

			if (!scope.TryLookup(variable.Name, out var lookup)) {
				diagnostics.AddDiagnostic(Diagnostic.ReportUndefinedVariable(token.Line, token.Column, variable));
				return null;
			}

			if (!isDeclaration) {
				if (lookup.IsReadOnly) {
					diagnostics.AddDiagnostic(Diagnostic.ReportReadonlyVariable(token.Line, token.Column, lookup));
					return null;
				}

				if(lookup.ValueType != variable.ValueType) {
					diagnostics.AddDiagnostic(Diagnostic.ReportVariableTypeMismatch(token.Line, token.Column, variable.Name, lookup.ValueType, variable.ValueType));
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

			diagnostics.AddDiagnostic(Diagnostic.ReportInvalidStatement(context.Start.Line, context.Start.Column, context.GetText()));
			return null;
		}
	}
}
