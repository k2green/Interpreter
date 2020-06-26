using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using InterpreterLib.Binding.Tree;
using InterpreterLib.Binding.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Binding {
	internal sealed class BinderOld : GLangBaseVisitor<BoundNode> {

		private BoundScope scope;
		private DiagnosticContainer diagnostics;

		private BinderOld(BoundScope parent) {
			scope = new BoundScope(parent);
			diagnostics = new DiagnosticContainer();
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
				// Bind the operand branch of the tree
				var operand = Visit(context.unaryExpression());

				// If this fails to produce a BoundExpression it has failed so we retun null
				if (operand == null || !(operand is BoundExpression))
					return null;

				// We bind the unary operator with the operator token text and the type of the BoundExpression
				var op = UnaryOperator.Bind(context.op.Text, ((BoundExpression)operand).ValueType);

				// If op has null the operator has failed to bind for the given types so this is reported as and error
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
			bool varDeclExists = context.variableDeclaration() != null;
			bool identifierExists = context.IDENTIFIER() != null;
			bool operatorExists = context.ASSIGNMENT_OPERATOR() != null;
			bool exprExists = context.expr != null;

			// Error cases for when certain tokens exist
			// varDeclExists == identifierExists checks that only assignments with exactly 1 of either an IDENTIFIER or variableDeclaration
			if (varDeclExists == identifierExists || !operatorExists || !exprExists) {
				diagnostics.AddDiagnostic(Diagnostic.ReportInvalidAssignment(context.Start.Line, context.Start.Column, context.GetText()));
				return null;
			}

			BoundNode exprNode = Visit(context.expr);

			if (exprNode == null || !(exprNode is BoundExpression))
				return null;

			var boundExpression = (BoundExpression)exprNode;
			VariableSymbol variable;

			// If the variableDeclaration exists
			if (varDeclExists) {
				// We try to bind the declaration statement
				var declStat = VisitVariableDeclaration(context.variableDeclaration(), boundExpression.ValueType, false);

				// A failure has occured in the binding of the variable declaration if declStat is null
				// An error should have already been reported so return null
				if (declStat == null)
					return null;

				// Set variable to the BoundVariable bound in the declaration statement.
				variable = declStat.VariableExpression.Variable;
			} else {					// If variableDeclaration doesn't exist, then then IDENTIFIER must exist
				// First we try to lookup the variable and assign it to the variable BoundVariable
				// If this fails report an undefined variable error.
				if (!scope.TryLookup(context.IDENTIFIER().GetText(), out variable)) {
					diagnostics.AddDiagnostic(Diagnostic.ReportUndefinedVariable(context.Start.Line, context.Start.Column, context.IDENTIFIER().GetText()));
					return null;
				}

				// If the variable found is readonly we also report a readonly variable error
				if (variable.IsReadOnly) {
					diagnostics.AddDiagnostic(Diagnostic.ReportReadonlyVariable(context.Start.Line, context.Start.Column, variable));
					return null;
				}
			}

			// As a final check we make sure the type of variable matches the type of the bound expression
			if (variable.ValueType != boundExpression.ValueType) {
				diagnostics.AddDiagnostic(Diagnostic.ReportVariableTypeMismatch(context.Start.Line, context.Start.Column, variable.Name, variable.ValueType, boundExpression.ValueType));
				return null;
			}

			return new BoundAssignmentExpression(variable, boundExpression);
		}

		public override BoundNode VisitVariableDeclaration([NotNull] GLangParser.VariableDeclarationContext context) {
			return VisitVariableDeclaration(context, TypeSymbol.Integer, true);
		}

		private BoundDeclarationStatement VisitVariableDeclaration([NotNull] GLangParser.VariableDeclarationContext context, TypeSymbol type, bool requireType) {
			bool declareationExists = context.DECL_VARIABLE() != null;
			bool identifierExists = context.IDENTIFIER() != null;
			bool delimeterExists = context.TYPE_DELIMETER() != null;
			bool typeNameExists = context.TYPE_NAME() != null;
			bool isReadOnly;

			/*  
			 *  Report error cases for when tokens don't exist
			 *  
			 *  delimeterExists ^ typeNameExists
			 *		checks for cases where only one of TYPE_DELIMETER and TYPE_NAME exist
			 *  (requireType && !delimeterExists && !typeNameExists) 
			 *		checks for cases where TYPE_DELIMETER and TYPE_NAME dont exist, but only when requireType is true
			 */
			if (!declareationExists || !identifierExists || delimeterExists ^ typeNameExists || (requireType && !delimeterExists && !typeNameExists)) {
				diagnostics.AddDiagnostic(Diagnostic.ReportInvalidDeclaration(context.Start.Line, context.Start.Column, context.GetText()));
				return null;
			}

			// Match the TYPE_NAME token for the declaration type. Report an error if this fails.
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

			// If both TYPE_DELIMETER and TYPE_NAME exist 
			if (delimeterExists && typeNameExists) {
				// type variable to the corresponding Type. Report an error if this fails.
				switch (context.TYPE_NAME().GetText()) {
					case "int":
						type = TypeSymbol.Integer;
						break;
					case "bool":
						type = TypeSymbol.Boolean;
						break;
					default:
						diagnostics.AddDiagnostic(Diagnostic.ReportInvalidTypeName(context.Start.Line, context.Start.Column, context.TYPE_NAME().GetText()));
						return null;
				}
			}

			// Create bound variable with the parsed information.
			var variable = new VariableSymbol(context.IDENTIFIER().GetText(), isReadOnly, type);

			if (!scope.TryDefine(variable)) {
				diagnostics.AddDiagnostic(Diagnostic.ReportRedefineVariable(context.Start.Line, context.Start.Column, scope[context.IDENTIFIER().GetText()], variable));
				return null;
			}

			return new BoundDeclarationStatement(new BoundVariableExpression(variable));
		}

		public override BoundNode VisitStatement([NotNull] GLangParser.StatementContext context) {
			if (context.forStat() != null)
				return Visit(context.forStat());

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

		public override BoundNode VisitForStat([NotNull] GLangParser.ForStatContext context) {
			var assignment = Visit(context.assign);
			var condition = Visit(context.condition);
			var step = Visit(context.step);
			var body = Visit(context.body);

			bool assignmentExists = assignment != null;
			bool condExists = condition != null;
			bool stepExists = step != null;
			bool bodyExists = body != null;

			if (!assignmentExists || !condExists || !stepExists || !bodyExists) {
				diagnostics.AddDiagnostic(Diagnostic.ReportInvalidFor(context.Start.Line, context.Start.Column, context.GetText()));
				return null;
			}

			if (!(assignment is BoundAssignmentExpression && condition is BoundBinaryExpression && step is BoundAssignmentExpression))
				return null;

			if(((BoundExpression)condition).ValueType != TypeSymbol.Boolean) {
				diagnostics.AddDiagnostic(Diagnostic.ReportInvalidType(context.Start.Line, context.Start.Column, ((BoundExpression)condition).ValueType, TypeSymbol.Boolean));
				return null;
			}

			return new BoundForStatement((BoundExpression)assignment, new BoundWhileStatement((BoundExpression)condition, new BoundBlock(new BoundNode[] { body, step })));
		}

		public override BoundNode VisitIfStat([NotNull] GLangParser.IfStatContext context) {
			if (context.IF() == null && context.L_PARENTHESIS() == null && context.condition == null && context.R_PARENTHESIS() == null && context.trueBranch == null) {
				diagnostics.AddDiagnostic(Diagnostic.ReportInvalidElse(context.Start.Line, context.Start.Column, context.GetText()));
				return null;
			}

			if (context.ELSE() == null ^ context.falseBranch == null) {
				diagnostics.AddDiagnostic(Diagnostic.ReportInvalidElse(context.Start.Line, context.Start.Column, context.GetText()));
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
			if (context.WHILE() != null && context.L_PARENTHESIS() != null && context.condition != null && context.R_PARENTHESIS() != null && context.body != null) {
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
