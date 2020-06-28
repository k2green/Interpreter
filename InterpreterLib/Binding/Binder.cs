using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using InterpreterLib.Binding.Tree;
using InterpreterLib.Binding.Types;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InterpreterLib.Binding {
	internal sealed class Binder : GLangBaseVisitor<BoundNode> {

		private BoundScope scope;
		private DiagnosticContainer diagnostics;

		private Binder(BoundScope parent) {
			scope = new BoundScope(parent);
			diagnostics = new DiagnosticContainer();
		}

		public static DiagnosticResult<BoundGlobalScope> BindGlobalScope(BoundGlobalScope prev, IParseTree tree) {
			var binder = new Binder(CreateParentScopes(prev));
			var res = binder.Bind(tree);
			var globScope = new BoundGlobalScope(prev, binder.scope.GetVariables(), res.Value);

			return new DiagnosticResult<BoundGlobalScope>(res.Diagnostics, globScope);
		}

		public static BoundScope CreateParentScopes(BoundGlobalScope previous) {
			if (previous == null)
				return null;

			var stack = new Stack<BoundGlobalScope>();
			while (previous != null) {
				stack.Push(previous);
				previous = previous.Previous;
			}

			BoundScope current = null;

			while (stack.Count > 0) {
				previous = stack.Pop();
				current = new BoundScope(current);

				foreach (var variable in previous.Variables)
					current.TryDefine(variable);
			}

			return current;
		}

		public DiagnosticResult<BoundNode> Bind(IParseTree tree) {
			return new DiagnosticResult<BoundNode>(diagnostics, Visit(tree));
		}

		private bool OnlyOne(IEnumerable<bool> conditions) => conditions.Count(b => b) == 1;
		private bool OnlyOne(params bool[] conditions) => OnlyOne((IEnumerable<bool>)conditions);

		private BoundError Error(Diagnostic diagnostic) {
			diagnostics.AddDiagnostic(diagnostic);

			return new BoundError(diagnostic, true, null);
		}

		private BoundError Error(Diagnostic diagnostic, bool isCausing, params BoundNode[] children) {
			if (isCausing && diagnostic != null)
				diagnostics.AddDiagnostic(diagnostic);

			return new BoundError(diagnostic, isCausing, children);
		}

		public override BoundNode VisitLiteral([NotNull] GLangParser.LiteralContext context) {
			if (context.INTEGER() != null)
				return new BoundLiteral(int.Parse(context.INTEGER().GetText()), TypeSymbol.Integer);

			if (context.BOOLEAN() != null)
				return new BoundLiteral(bool.Parse(context.BOOLEAN().GetText()), TypeSymbol.Boolean);

			var token = context.Start;

			if (context.IDENTIFIER() != null) {
				if (scope.TryLookup(context.IDENTIFIER().GetText(), out var variable)) {
					return new BoundVariableExpression(variable);
				} else {
					Error(Diagnostic.ReportUndefinedVariable(token.Line, token.Column, context.IDENTIFIER().GetText()));
				}
			}

			return Error(Diagnostic.ReportInvalidLiteral(token.Line, token.Column, context.GetText()));
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
				if (!(operand is BoundExpression)) {
					int line = context.unaryExpression().Start.Line;
					int column = context.unaryExpression().Start.Column;
					var text = context.unaryExpression().GetText();
					var diagnostic = Diagnostic.ReportFailedVisit(line, column, text);
					return Error(diagnostic, false, operand);
				}

				// We bind the unary operator with the operator token text and the type of the BoundExpression
				var op = UnaryOperator.Bind(context.op.Text, ((BoundExpression)operand).ValueType);

				// If op has null the operator has failed to bind for the given types so this is reported as and error
				if (op == null) {
					var diagnostic = Diagnostic.ReportInvalidUnaryOperator(context.op.Line, context.op.Column, context.op.Text, ((BoundExpression)operand).ValueType);
					return Error(diagnostic, true, operand);
				}

				return new BoundUnaryExpression(op, (BoundExpression)operand);
			}

			return Error(Diagnostic.ReportInvalidUnaryExpression(context.Start.Line, context.Start.Column, context.GetText()));
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
				if (!(left is BoundExpression) || !(right is BoundExpression)) {
					var token = context.Start;
					var diagnostic = Diagnostic.ReportFailedVisit(token.Line, token.Column, context.GetText());
					return Error(diagnostic, false, left, right);
				}

				// Type checking for operators
				var op = BinaryOperator.Bind(context.op.Text, ((BoundExpression)left).ValueType, ((BoundExpression)right).ValueType);

				if (op == null) {
					// Report the operator is invalid for the given types
					var diagnostic = Diagnostic.ReportInvalidBinaryOperator(context.op.Line, context.op.Column, context.op.Text, ((BoundExpression)left).ValueType, ((BoundExpression)right).ValueType);
					return Error(diagnostic, true, left, right);
				}

				// Return a new bound binary expression.
				return new BoundBinaryExpression((BoundExpression)left, op, (BoundExpression)right);
			}

			return Error(Diagnostic.ReportInvalidBinaryExpression(context.Start.Line, context.Start.Column, context.GetText()));
		}

		public override BoundNode VisitAssignmentExpression([NotNull] GLangParser.AssignmentExpressionContext context) {
			bool hasIdentifier = context.IDENTIFIER() != null;
			bool hasAssignmentOp = context.ASSIGNMENT_OPERATOR() != null && context.ASSIGNMENT_OPERATOR().GetText().Equals("=");
			bool hasExpression = context.binaryExpression() != null;

			if (!hasIdentifier || !hasAssignmentOp || !hasExpression)
				return Error(Diagnostic.ReportInvalidAssignment(context.Start.Line, context.Start.Column, context.GetText()));

			var exprRule = context.binaryExpression();
			var exprNodeResult = Visit(context.binaryExpression());

			if (!(exprNodeResult is BoundExpression)) {
				var diagnostic = Diagnostic.ReportFailedVisit(exprRule.Start.Line, exprRule.Start.Column, exprRule.GetText());
				return Error(diagnostic, false, exprNodeResult);
			}

			var expression = (BoundExpression)exprNodeResult;
			if (!scope.TryLookup(context.IDENTIFIER().GetText(), out var variable)) {
				var diagnostic = Diagnostic.ReportUndefinedVariable(context.Start.Line, context.Start.Column, context.IDENTIFIER().GetText());
				return Error(diagnostic, false, exprNodeResult);
			}

			if (variable.IsReadOnly) {
				var diagnostic = Diagnostic.ReportReadonlyVariable(context.Start.Line, context.Start.Column, variable);
				return Error(diagnostic, true, exprNodeResult);
			}

			if (expression.ValueType != variable.ValueType) {
				var diagnostic = Diagnostic.ReportCannotRedefine(context.Start.Line, context.Start.Column, variable.Name, variable.ValueType, expression.ValueType);
				return Error(diagnostic, true, exprNodeResult);
			}

			return new BoundAssignmentExpression(variable, expression);
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

			if (context.variableDeclarationStatement() != null)
				return Visit(context.variableDeclarationStatement());

			if (context.assignmentExpression() != null)
				return Visit(context.assignmentExpression());

			if (context.expressionStatement() != null)
				return Visit(context.expressionStatement());


			return Error(Diagnostic.ReportInvalidStatement(context.Start.Line, context.Start.Column, context.GetText()));
		}

		public override BoundNode VisitVariableDeclarationStatement([NotNull] GLangParser.VariableDeclarationStatementContext context) {
			bool hasIdentifier = context.DECL_VARIABLE() != null && context.IDENTIFIER() != null;
			bool hasTypeDef = context.TYPE_DELIMETER() != null && context.TYPE_NAME() != null;
			bool hasExpression = context.ASSIGNMENT_OPERATOR() != null && context.ASSIGNMENT_OPERATOR().GetText().Equals("=") && context.binaryExpression() != null;
			bool isReadOnly;
			TypeSymbol type;
			BoundExpression initialiser;

			if (!hasIdentifier || !OnlyOne(hasTypeDef, hasExpression))
				return Error(Diagnostic.ReportInvalidDeclaration(context.Start.Line, context.Start.Column, context.GetText()));

			var declVarToken = context.DECL_VARIABLE().Symbol;
			switch (declVarToken.Text) {
				case "var":
					isReadOnly = false;
					break;
				case "val":
					isReadOnly = true;
					break;

				default: return Error(Diagnostic.ReportInvalidDeclarationKeyword(declVarToken.Line, declVarToken.Column, declVarToken.Text));
			}

			if (hasTypeDef) {
				var typeNameToken = context.TYPE_NAME().Symbol;
				initialiser = null;

				switch (typeNameToken.Text) {
					case "int":
						type = TypeSymbol.Integer;
						break;
					case "bool":
						type = TypeSymbol.Boolean;
						break;

					default: return Error(Diagnostic.ReportInvalidTypeName(typeNameToken.Line, typeNameToken.Column, typeNameToken.Text));
				}
			} else {
				var exprRule = context.binaryExpression();
				var initialVisit = Visit(exprRule);

				if (!(initialVisit is BoundExpression)) {
					var diagnostic = Diagnostic.ReportFailedVisit(exprRule.Start.Line, exprRule.Start.Column, exprRule.GetText());
					return Error(diagnostic, false, initialVisit);
				}

				initialiser = (BoundExpression)initialVisit;
				type = initialiser.ValueType;
			}

			var identifierToken = context.IDENTIFIER().Symbol;
			var variable = new VariableSymbol(identifierToken.Text, isReadOnly, type);

			if (!scope.TryDefine(variable)) {
				var diagnostic = Diagnostic.ReportCannotDefine(identifierToken.Line, identifierToken.Column, identifierToken.Text);
				return Error(diagnostic, true, initialiser);
			}

			return new BoundVariableDeclarationStatement(variable, initialiser);
		}

		public override BoundNode VisitExpressionStatement([NotNull] GLangParser.ExpressionStatementContext context) {
			if (context.binaryExpression() == null)
				return Error(Diagnostic.ReportInvalidExpressionStatement(context.Start.Line, context.Start.Column, context.GetText()));

			var exprCtx = context.binaryExpression();
			var initVisit = Visit(exprCtx);

			if (!(initVisit is BoundExpression)) {
				var diagnostic = Diagnostic.ReportFailedVisit(exprCtx.Start.Line, exprCtx.Start.Column, exprCtx.GetText());
				return Error(diagnostic, false, initVisit);
			}

			return new BoundExpressionStatement((BoundExpression)initVisit);
		}

		public override BoundNode VisitBlock([NotNull] GLangParser.BlockContext context) {
			if (context.L_BRACE() == null || context.statement() == null || context.R_BRACE() == null)
				return Error(Diagnostic.ReportInvalidBlock(context.Start.Line, context.Start.Column, context.GetText()));

			List<BoundStatement> statements = new List<BoundStatement>();
			foreach (var stat in context.statement()) {
				var nextVisit = Visit(stat);

				if (!(nextVisit is BoundStatement)) {
					var diagnostic = Diagnostic.ReportFailedVisit(stat.Start.Line, stat.Start.Column, stat.GetText());
					return Error(diagnostic, false, statements.ToArray());
				}

				statements.Add((BoundStatement)nextVisit);
			}

			return new BoundBlock(statements);
		}

		public override BoundNode VisitForAssign([NotNull] GLangParser.ForAssignContext context) {
			bool hasDeclaration = context.variableDeclarationStatement() != null;
			bool hasAssignment = context.assignmentExpression() != null;

			if (hasDeclaration == hasAssignment)
				return Error(Diagnostic.ReportInvalidFor(context.Start.Line, context.Start.Column, context.GetText()));

			if (hasDeclaration) {
				return Visit(context.variableDeclarationStatement());
			} else {
				return Visit(context.assignmentExpression());
			}
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
				return Error(Diagnostic.ReportInvalidFor(context.Start.Line, context.Start.Column, context.GetText()));
			}

			if (!((assignment is BoundAssignmentExpression || assignment is BoundVariableDeclarationStatement) && condition is BoundBinaryExpression && step is BoundAssignmentExpression && body is BoundStatement)) {
				var diagnostic = Diagnostic.ReportFailedVisit(context.Start.Line, context.Start.Column, context.GetText());
				return Error(diagnostic, false, assignment, condition, step, body);
			}

			if (condition is BoundExpression && ((BoundExpression)condition).ValueType != TypeSymbol.Boolean) {
				var diagnostic = Diagnostic.ReportInvalidType(context.Start.Line, context.Start.Column, ((BoundExpression)condition).ValueType, TypeSymbol.Boolean);
				return Error(diagnostic, true, assignment, condition, step, body);
			}

			return new BoundForStatement(assignment, (BoundExpression)condition, (BoundExpression)step, (BoundStatement)body);
		}

		public override BoundNode VisitIfStat([NotNull] GLangParser.IfStatContext context) {
			if (context.IF() == null && context.L_PARENTHESIS() == null && context.condition == null && context.R_PARENTHESIS() == null && context.trueBranch == null) {
				return Error(Diagnostic.ReportInvalidIf(context.Start.Line, context.Start.Column, context.GetText()));
			}

			if (context.ELSE() == null ^ context.falseBranch == null) {
				return Error(Diagnostic.ReportMalformedElse(context.Start.Line, context.Start.Column, context.GetText()));
			}

			var condition = Visit(context.condition);

			scope = new BoundScope(scope);
			var trueBrStat = Visit(context.trueBranch);
			scope = scope.Parent;


			BoundNode falseBrStat = null;

			if (context.ELSE() != null) {
				scope = new BoundScope(scope);
				falseBrStat = Visit(context.falseBranch);
				scope = scope.Parent;
			}

			if (!(condition is BoundExpression)) {
				var diagnostic = Diagnostic.ReportFailedVisit(context.condition.Start.Line, context.condition.Start.Column, context.condition.GetText());
				return Error(diagnostic, false, condition, trueBrStat, falseBrStat);
			}

			if (!(trueBrStat is BoundStatement)) {
				var diagnostic = Diagnostic.ReportFailedVisit(context.trueBranch.Start.Line, context.trueBranch.Start.Column, context.trueBranch.GetText());
				return Error(diagnostic, false, condition, trueBrStat, falseBrStat);
			}

			if (!(falseBrStat is BoundStatement)) {
				var diagnostic = Diagnostic.ReportFailedVisit(context.falseBranch.Start.Line, context.falseBranch.Start.Column, context.falseBranch.GetText());
				return Error(diagnostic, false, condition, trueBrStat, falseBrStat);
			}

			return new BoundIfStatement((BoundExpression)condition, (BoundStatement)trueBrStat, (BoundStatement)falseBrStat);
		}

		public override BoundNode VisitWhileStat([NotNull] GLangParser.WhileStatContext context) {
			if (context.WHILE() != null && context.L_PARENTHESIS() != null && context.condition != null && context.R_PARENTHESIS() != null && context.body != null) {
				var expressionNode = Visit(context.condition);

				if (!(expressionNode is BoundExpression)) {
					var token = context.condition.Start;
					var diagnostic = Diagnostic.ReportFailedVisit(token.Line, token.Column, context.condition.GetText());
					return Error(diagnostic, false, expressionNode);
				}

				var expression = (BoundExpression)expressionNode;

				if (expression.ValueType != TypeSymbol.Boolean) {
					var token = context.condition.Start;
					var diagnostic = Diagnostic.ReportInvalidType(token.Line, token.Column, expression.ValueType, TypeSymbol.Boolean);
					return Error(diagnostic, true, expressionNode);
				}

				scope = new BoundScope(scope);
				var body = Visit(context.body);
				scope = scope.Parent;

				if (!(body is BoundStatement)) {
					var token = context.body.Start;
					var diagnostic = Diagnostic.ReportFailedVisit(token.Line, token.Column, context.body.GetText());
					return Error(diagnostic, false, expressionNode);
				}

				return new BoundWhileStatement(expression, (BoundStatement)body);
			}

			return Error(Diagnostic.ReportInvalidWhile(context.Start.Line, context.Start.Column, context.GetText()));
		}
	}
}
