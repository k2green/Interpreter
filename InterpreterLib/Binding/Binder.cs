using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using InterpreterLib.Binding.Tree;
using InterpreterLib.Binding.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Binding {
	internal sealed class Binder : GLangBaseVisitor<BoundNode> {

		private BoundScope scope;
		private DiagnosticContainer diagnostics;

		private Binder(BoundScope parent) {
			scope = new BoundScope(parent);
			diagnostics = new DiagnosticContainer();
		}

		public static DiagnosticResult<BoundGlobalScope> BindGlobalScope(BoundGlobalScope previous, IParseTree tree) {
			Binder binder = new Binder(CreateParentScopes(previous));

			BoundNode root = binder.Visit(tree);

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

		public override BoundNode VisitLiteral([NotNull] GLangParser.LiteralContext context) {

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

		public override BoundNode VisitUnaryExpression([NotNull] GLangParser.UnaryExpressionContext context) {
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

				if (operand == null || !(operand is BoundExpression))
					return null;

				var op = UnaryOperator.Bind(context.op.Text, ((BoundExpression)operand).ValueType);

				if (op == null) {
					diagnostics.AddDiagnostic(Diagnostic.ReportInvalidUnaryOperator(context.op.Line, context.op.Column, context.op.Text, ((BoundExpression)operand).ValueType));
					return null;
				}

				return new BoundUnaryExpression(op, (BoundExpression)operand);
			}

			diagnostics.AddDiagnostic(Diagnostic.ReportInvalidSyntax(context.Start.Line, context.Start.Column, $"Unary Expression {context.GetText()}"));
			return null;
		}

		public override BoundNode VisitBinaryExpression([NotNull] GLangParser.BinaryExpressionContext context) {
			// Return the binding of the base case
			if (context.atom != null)
				return Visit(context.atom);

			// Handles expressions with sub-expressions
			if (context.left != null && context.op != null && context.right != null) {
				var left = Visit(context.left);
				var right = Visit(context.right);

				// Return null if the binding of either child fails
				if (left == null || right == null || !(left is BoundExpression) || !(right is BoundExpression))
					return null;

				// Type checking for operators
				var op = BinaryOperator.Bind(context.op.Text, ((BoundExpression)left).ValueType, ((BoundExpression)right).ValueType);

				if (op == null) {
					// Report the operator is invalid for the given types
					diagnostics.AddDiagnostic(Diagnostic.ReportInvalidBinaryOperator(context.op.Line, context.op.Column, context.op.Text, ((BoundExpression)left).ValueType, ((BoundExpression)right).ValueType));
					return null;
				}

				// Return a new bound binary expression.
				return new BoundBinaryExpression((BoundExpression)left, op, (BoundExpression)right);
			}

			diagnostics.AddDiagnostic(Diagnostic.ReportInvalidSyntax(context.Start.Line, context.Start.Column, $"Binary Expression {context.GetText()}"));
			return null;
		}

		public override BoundNode VisitAssignmentStatement([NotNull] GLangParser.AssignmentStatementContext context) {
			return VisitAssignmentStatement(context, false, false);
		}

		private BoundNode VisitAssignmentStatement([NotNull] GLangParser.AssignmentStatementContext context, bool isDeclaration, bool isReadOnly) {
			bool varDeclExists = context.variableDeclaration() != null;
			bool identifierExists = context.IDENTIFIER() != null;
			bool operatorExists = context.ASSIGNMENT_OPERATOR() != null;
			bool exprExists = context.expr != null;

			if (varDeclExists == identifierExists || !operatorExists || !exprExists) {
				diagnostics.AddDiagnostic(Diagnostic.ReportInvalidAssignment(context.Start.Line, context.Start.Column, context.GetText()));
				return null;
			}

			BoundNode exprNode = Visit(context.expr);

			if (exprNode == null || !(exprNode is BoundExpression))
				return null;

			var boundExpression = (BoundExpression)exprNode;
			BoundVariable variable;

			if (varDeclExists) {
				var declStat = VisitVariableDeclaration(context.variableDeclaration(), boundExpression.ValueType, false);

				if (declStat == null)
					return null;

				variable = declStat.VariableExpression.Variable;
			} else {
				if (!scope.TryLookup(context.IDENTIFIER().GetText(), out variable)) {
					diagnostics.AddDiagnostic(Diagnostic.ReportUndefinedVariable(context.Start.Line, context.Start.Column, context.IDENTIFIER().GetText()));
					return null;
				}
			}

			return new BoundAssignmentExpression(variable, boundExpression);
		}

		public override BoundNode VisitVariableDeclaration([NotNull] GLangParser.VariableDeclarationContext context) {
			return VisitVariableDeclaration(context, BoundType.Integer, true);
		}

		private BoundDeclarationStatement VisitVariableDeclaration([NotNull] GLangParser.VariableDeclarationContext context, BoundType type, bool requireType) {
			bool declareationExists = context.DECL_VARIABLE() != null;
			bool identifierExists = context.IDENTIFIER() != null;
			bool delimeterExists = context.TYPE_DELIMETER() != null;
			bool typeNameExists = context.TYPE_NAME() != null;
			bool isReadOnly;

			if (!declareationExists || !identifierExists || delimeterExists ^ typeNameExists || (requireType && !delimeterExists && !typeNameExists)) {
				diagnostics.AddDiagnostic(Diagnostic.ReportInvalidDeclaration(context.Start.Line, context.Start.Column, context.GetText()));
				return null;
			}

			switch (context.DECL_VARIABLE().GetText()) {
				case "var":
					isReadOnly = false;
					break;
				case "val":
					isReadOnly = true;
					break;

				default:
					diagnostics.AddDiagnostic(Diagnostic.ReportInvalidDeclaration(context.Start.Line, context.Start.Column, context.DECL_VARIABLE().GetText()));
					return null;
			}

			if (delimeterExists && typeNameExists) {
				switch (context.TYPE_NAME().GetText()) {
					case "int":
						type = BoundType.Integer;
						break;
					case "bool":
						type = BoundType.Boolean;
						break;
					default:
						diagnostics.AddDiagnostic(Diagnostic.ReportInvalidTypeName(context.Start.Line, context.Start.Column, context.TYPE_NAME().GetText()));
						return null;
				}
			}

			var variable = new BoundVariable(context.IDENTIFIER().GetText(), isReadOnly, type);

			if (!scope.TryDefine(variable)) {
				diagnostics.AddDiagnostic(Diagnostic.ReportRedefineVariable(context.Start.Line, context.Start.Column, scope[context.IDENTIFIER().GetText()], variable));
				return null;
			}

			return new BoundDeclarationStatement(new BoundVariableExpression(variable));
		}

		public override BoundNode VisitStatement([NotNull] GLangParser.StatementContext context) {
			if (context.whileStat() != null)
				return Visit(context.whileStat());

			if (context.ifStat() != null)
				return Visit(context.ifStat());

			if (context.block() != null)
				return Visit(context.block());

			if (context.assignmentStatement() != null)
				return Visit(context.assignmentStatement());

			if (context.variableDeclaration() != null)
				return Visit(context.variableDeclaration());

			if (context.binaryExpression() != null)
				return Visit(context.binaryExpression());

			diagnostics.AddDiagnostic(Diagnostic.ReportInvalidStatement(context.Start.Line, context.Start.Column, context.GetText()));
			return null;
		}

		public override BoundNode VisitBlock([NotNull] GLangParser.BlockContext context) {
			List<BoundNode> statements = new List<BoundNode>();
			foreach (var stat in context.statement())
				statements.Add(Visit(stat));

			return new BoundBlock(statements);
		}

		public override BoundNode VisitIfStat([NotNull] GLangParser.IfStatContext context) {
			if (context.IF() == null && context.L_BRACKET() == null && context.condition == null && context.R_BRACKET() == null && context.trueBranch == null) {
				diagnostics.ReportInvalidElse(context.Start.Line, context.Start.Column, context.GetText());
				return null;
			}

			if (context.ELSE() == null ^ context.falseBranch == null) {
				diagnostics.ReportInvalidElse(context.Start.Line, context.Start.Column, context.GetText());
				return null;
			}

			var condition = (BoundExpression)Visit(context.condition);

			scope = new BoundScope(scope);
			var trueBrStat = Visit(context.trueBranch);
			scope = scope.Parent;


			BoundNode falseBrStat = null;

			if (context.ELSE() != null) {
				scope = new BoundScope(scope);
				falseBrStat = Visit(context.falseBranch);
				scope = scope.Parent;
			}

			return new BoundIfStatement(condition, trueBrStat, falseBrStat);
		}

		public override BoundNode VisitWhileStat([NotNull] GLangParser.WhileStatContext context) {
			if (context.WHILE() != null && context.L_BRACKET() != null && context.condition != null && context.R_BRACKET() != null && context.body != null) {
				var expression = (BoundExpression)Visit(context.condition);

				scope = new BoundScope(scope);
				var body = Visit(context.body);
				scope = scope.Parent;

				return new BoundWhileStatement(expression, body);
			}

			diagnostics.AddDiagnostic(Diagnostic.ReportInvalidWhile(context.Start.Line, context.Start.Column, context.GetText()));
			return null;
		}
	}
}
