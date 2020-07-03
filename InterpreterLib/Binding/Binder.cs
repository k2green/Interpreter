using InterpreterLib.Binding.Lowering;
using InterpreterLib.Binding.Tree;
using InterpreterLib.Binding.Tree.Expressions;
using InterpreterLib.Binding.Tree.Statements;
using InterpreterLib.Binding.Types;
using InterpreterLib.Syntax.Tree;
using InterpreterLib.Syntax.Tree.Expressions;
using InterpreterLib.Syntax.Tree.Statements;
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

		public static DiagnosticResult<BoundGlobalScope> BindGlobalScope(BoundGlobalScope prev, SyntaxNode tree) {
			var binder = new Binder(CreateParentScopes(prev));
			var res = binder.BindRoot(tree);
			BoundGlobalScope globScope;

			if (!(res.Value is BoundStatement statement)) {
				return new DiagnosticResult<BoundGlobalScope>(res.Diagnostics, prev);
			}

			if (res.Diagnostics.Any()) {
				globScope = new BoundGlobalScope(prev, binder.scope.GetVariables(), new BoundStatement[] { statement });
				return new DiagnosticResult<BoundGlobalScope>(res.Diagnostics, globScope);
			}

			var lowered = Lowerer.Lower(statement);
			globScope = new BoundGlobalScope(prev, binder.scope.GetVariables(), lowered.Statements.ToArray());
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

		public DiagnosticResult<BoundNode> BindRoot(SyntaxNode node) {
			var boundNode = Bind(node);

			return new DiagnosticResult<BoundNode>(diagnostics, boundNode);
		}

		private BoundNode Error(Diagnostic diagnostic, bool addToDiagnostics = true) {
			if (addToDiagnostics)
				diagnostics.AddDiagnostic(diagnostic);

			return new BoundError(diagnostic);
		}

		private BoundNode Bind(SyntaxNode syntax) {
			switch (syntax.Type) {
				case SyntaxType.Literal:
					return BindLiteral((LiteralSyntax)syntax);
				case SyntaxType.UnaryExpression:
					return BindUnaryExpression((UnaryExpressionSyntax)syntax);
				case SyntaxType.BinaryExpression:
					return BindBinaryExpression((BinaryExpressionSyntax)syntax);
				case SyntaxType.Declaration:
					return BindDeclarationStatement((VariableDeclarationSyntax)syntax);
				case SyntaxType.Variable:
					return BindVariableExpression((VariableSyntax)syntax);
				case SyntaxType.Assignment:
					return BindAssignmentExpression((AssignmentExpressionSyntax)syntax);
				case SyntaxType.Expression:
					return BindExpressionStatement((ExpressionStatementSyntax)syntax);
				case SyntaxType.IfStatement:
					return BindIfStatement((IfStatementSyntax)syntax);
				case SyntaxType.WhileLoop:
					return BindWhileLoop((WhileLoopSyntax)syntax);
				case SyntaxType.ForLoop:
					return BindForLoop((ForLoopSyntax)syntax);
				case SyntaxType.Error:
					return BindError((ErrorSyntax)syntax);
				case SyntaxType.Block:
					return BindBlock((BlockSyntax)syntax);
				default: throw new Exception($"Encountered unhandled syntax {syntax.Type}");
			}
		}

		private BoundNode BindBlock(BlockSyntax syntax) {
			var statements = new List<BoundStatement>();

			foreach (var statSyntax in syntax.Statements) {
				var bound = Bind(statSyntax);

				if (!(bound is BoundStatement statement))
					return bound;

				statements.Add(statement);
			}

			return new BoundBlock(statements);
		}

		private BoundNode BindError(ErrorSyntax syntax) {
			return Error(syntax.Diagnostic, false);
		}

		private BoundNode BindForLoop(ForLoopSyntax syntax) {
			var assignVisit = Bind(syntax.Assignment);
			var conditionVisit = Bind(syntax.Condition);
			var stepVisit = Bind(syntax.Step);
			var bodyVisit = Bind(syntax.Body);

			if (!(assignVisit is BoundStatement assignment))
				return assignVisit;

			if (!(bodyVisit is BoundExpression condition))
				return bodyVisit;

			if (!(conditionVisit is BoundExpression step))
				return conditionVisit;

			if (!(stepVisit is BoundStatement body))
				return stepVisit;

			if (condition.ValueType != TypeSymbol.Boolean)
				return Error(Diagnostic.ReportInvalidType(syntax.Condition.Span.Line, syntax.Condition.Span.Column, syntax.Condition.ToString(), TypeSymbol.Boolean));

			return new BoundForStatement(assignment, condition, step, body);
		}

		private BoundNode BindWhileLoop(WhileLoopSyntax syntax) {
			var conditionVisit = Bind(syntax.Condition);
			var bodyVisit = Bind(syntax.Body);

			if (!(conditionVisit is BoundExpression condition))
				return conditionVisit;

			if (!(bodyVisit is BoundStatement body))
				return bodyVisit;

			if (condition.ValueType != TypeSymbol.Boolean)
				return Error(Diagnostic.ReportInvalidType(syntax.Condition.Span.Line, syntax.Condition.Span.Column, syntax.Condition.ToString(), TypeSymbol.Boolean));

			return new BoundWhileStatement(condition, body);
		}

		private BoundNode BindIfStatement(IfStatementSyntax syntax) {
			var conditionVisit = Bind(syntax.Condition);
			var trueBranchVisit = Bind(syntax.TrueBranch);
			var falseBranchVisit = syntax.FalseBranch == null ? null : Bind(syntax.FalseBranch);

			if (!(conditionVisit is BoundExpression boundCondition))
				return conditionVisit;

			if (!(trueBranchVisit is BoundStatement boundTrueBr))
				return trueBranchVisit;

			if (falseBranchVisit != null && !(falseBranchVisit is BoundStatement))
				return falseBranchVisit;

			var boundFalseBr = falseBranchVisit == null ? null : (BoundStatement)falseBranchVisit;

			if (boundCondition.ValueType != TypeSymbol.Boolean)
				return Error(Diagnostic.ReportInvalidType(syntax.Condition.Span.Line, syntax.Condition.Span.Column, syntax.Condition.ToString(), TypeSymbol.Boolean));

			return new BoundIfStatement(boundCondition, boundTrueBr, boundFalseBr);
		}

		private BoundNode BindExpressionStatement(ExpressionStatementSyntax syntax) {
			var expressionVisit = Bind(syntax.Expression);

			if (!(expressionVisit is BoundExpression expression))
				return expressionVisit;

			return new BoundExpressionStatement(expression);
		}

		private BoundNode BindAssignmentExpression(AssignmentExpressionSyntax syntax) {
			if (syntax.Definition != null)
				return Error(Diagnostic.ReportAssingmentTypeDef(syntax.Definition.Span.Line, syntax.Definition.Span.Column, syntax.Definition.ToString()));

			string identifierText = syntax.IdentifierToken.Token.Text;
			var boundExpression = Bind(syntax.Expression);

			if (!(boundExpression is BoundExpression expression))
				return boundExpression;

			if (!scope.TryLookup(identifierText, out var variable))
				return Error(Diagnostic.ReportUndefinedVariable(syntax.IdentifierToken.Span.Line, syntax.IdentifierToken.Span.Column, identifierText));

			if (variable.ValueType != expression.ValueType)
				return Error(Diagnostic.ReportVariableTypeMismatch(syntax.IdentifierToken.Span.Line, syntax.IdentifierToken.Span.Column, identifierText, variable.ValueType, expression.ValueType));


			return new BoundAssignmentExpression(variable, expression);
		}

		private BoundNode BindVariableExpression(VariableSyntax syntax) {
			string varaibleName = syntax.IdentifierToken.Token.Text;

			if (!scope.TryLookup(varaibleName, out var variable))
				return Error(Diagnostic.ReportUndefinedVariable(syntax.IdentifierToken.Span.Line, syntax.IdentifierToken.Span.Column, varaibleName));

			return new BoundVariableExpression(variable);
		}

		private BoundNode BindDeclarationStatement(VariableDeclarationSyntax syntax) {
			var declText = syntax.KeywordToken.Token.Text;
			var identifierText = syntax.IdentifierToken.Token.Text;

			bool isreadOnly;
			TypeSymbol type = null;
			BoundExpression initialiser = null;

			switch (declText) {
				case "var":
					isreadOnly = false;
					break;
				case "val":
					isreadOnly = true;
					break;

				default:
					return Error(Diagnostic.ReportUnknownDeclKeyword(syntax.KeywordToken.Span.Line, syntax.KeywordToken.Span.Column, syntax.KeywordToken.ToString()));
			}

			if (syntax.Initialiser != null) {
				var initialiserBind = Bind(syntax.Initialiser);

				if (!(initialiserBind is BoundExpression init))
					return initialiserBind;

				initialiser = init;
				type = initialiser.ValueType;
			}

			if (syntax.Definition != null) {
				switch (syntax.Definition.NameToken.Token.Text) {
					case "int":
						type = TypeSymbol.Integer;
						break;
					case "bool":
						type = TypeSymbol.Boolean;
						break;

					default:
						return Error(Diagnostic.ReportUnknownTypeKeyword(syntax.KeywordToken.Span.Line, syntax.KeywordToken.Span.Column, syntax.KeywordToken.ToString()));
				}
			}

			if (type != null && initialiser != null && initialiser.ValueType != type)
				return Error(Diagnostic.ReportCannotCast(syntax.Definition.Span.Line, syntax.Definition.Span.Column, initialiser.ValueType, type));

			var variable = new VariableSymbol(identifierText, isreadOnly, type);

			if (!scope.TryDefine(variable))
				return Error(Diagnostic.ReportCannotRedefine(syntax.IdentifierToken.Span.Line, syntax.IdentifierToken.Span.Column, identifierText));

			return new BoundVariableDeclarationStatement(variable, initialiser);
		}

		private BoundNode BindBinaryExpression(BinaryExpressionSyntax syntax) {
			var opToken = syntax.OpToken.Token;
			var opText = opToken.Text;

			var boundLeftVisit = Bind(syntax.LeftSyntax);
			var boundRightVisit = Bind(syntax.RightSyntax);

			if (!(boundLeftVisit is BoundExpression left))
				return boundLeftVisit;
			if (!(boundRightVisit is BoundExpression right))
				return boundRightVisit;

			var op = BinaryOperator.Bind(opText, left.ValueType, right.ValueType);

			if (op == null)
				return Error(Diagnostic.ReportInvalidOperator(opToken.Line, opToken.Column, opText, left.ValueType, right.ValueType));

			return new BoundBinaryExpression(left, op, right);
		}

		private BoundNode BindUnaryExpression(UnaryExpressionSyntax syntax) {
			var opToken = syntax.OpToken.Token;
			var opText = opToken.Text;
			var boundSubExpressionVisit = Bind(syntax.Expression);

			if (!(boundSubExpressionVisit is BoundExpression boundSubExpression))
				return boundSubExpressionVisit;

			var op = UnaryOperator.Bind(opText, boundSubExpression.ValueType);

			if (op == null)
				return Error(Diagnostic.ReportInvalidOperator(opToken.Line, opToken.Column, opText, boundSubExpression.ValueType));

			return new BoundUnaryExpression(op, boundSubExpression);
		}

		private BoundNode BindLiteral(LiteralSyntax syntax) {
			var literalText = syntax.LiteralToken.Token.Text;

			if (int.TryParse(literalText, out int intVal))
				return new BoundLiteral(intVal, TypeSymbol.Integer);

			if (bool.TryParse(literalText, out bool boolVal))
				return new BoundLiteral(boolVal, TypeSymbol.Boolean);

			var stringText = syntax.ToString();
			var text = stringText.Substring(1, stringText.Length - 2);
			return new BoundLiteral(text, TypeSymbol.String);
		}
	}
}
