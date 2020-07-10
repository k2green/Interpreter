using InterpreterLib.Binding.Lowering;
using InterpreterLib.Binding.Tree;
using InterpreterLib.Binding.Tree.Expressions;
using InterpreterLib.Binding.Tree.Statements;
using InterpreterLib.Types;
using InterpreterLib.Syntax.Tree;
using InterpreterLib.Syntax.Tree.Expressions;
using InterpreterLib.Syntax.Tree.Global;
using InterpreterLib.Syntax.Tree.Statements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Immutable;
using InterpreterLib.Syntax;

namespace InterpreterLib.Binding {
	internal sealed class Binder : GLangBaseVisitor<BoundNode> {
		public DiagnosticContainer Diagnostics { get; }

		private BoundScope scope;
		private FunctionSymbol Function;
		private Dictionary<FunctionSymbol, FunctionDeclarationSyntax> functionBodies;

		private Stack<(LabelSymbol, LabelSymbol)> breakContinueLabels;
		private int currentBreakContinueNo;

		private Binder(BoundScope parent, FunctionSymbol function = null) {
			scope = new BoundScope(parent);
			Diagnostics = new DiagnosticContainer();
			Function = function;
			functionBodies = new Dictionary<FunctionSymbol, FunctionDeclarationSyntax>();
			breakContinueLabels = new Stack<(LabelSymbol, LabelSymbol)>();

			if (function != null) {
				foreach (var param in function.Parameters) {
					scope.TryDefineVariable(new VariableSymbol(param.Name, false, param.ValueType));
				}
			}
		}

		public static DiagnosticResult<BoundGlobalScope> BindGlobalScope(BoundGlobalScope prev, CompilationUnitSyntax tree) {
			var binder = new Binder(CreateParentScopes(prev));
			var res = binder.BindCompilationUnit(tree);
			BoundGlobalScope globScope;

			if (res.Diagnostics.Any()) {
				globScope = new BoundGlobalScope(prev, binder.scope.GetVariables(), binder.scope.GetFunctions(), null, binder.functionBodies);
				return new DiagnosticResult<BoundGlobalScope>(binder.Diagnostics, globScope);
			}

			var lowered = Lowerer.Lower(res.Value);
			globScope = new BoundGlobalScope(prev, binder.scope.GetVariables(), binder.scope.GetFunctions(), lowered, binder.functionBodies);
			return new DiagnosticResult<BoundGlobalScope>(binder.Diagnostics, globScope);
		}

		public static BoundScope CreateParentScopes(BoundGlobalScope previous) {
			if (previous == null)
				return CreateBaseScope();

			var stack = new Stack<BoundGlobalScope>();
			while (previous != null) {
				stack.Push(previous);
				previous = previous.Previous;
			}

			BoundScope current = CreateBaseScope();

			while (stack.Count > 0) {
				previous = stack.Pop();
				current = new BoundScope(current);

				foreach (var variable in previous.Variables)
					current.TryDefineVariable(variable);
			}

			return current;
		}

		private static BoundScope CreateBaseScope() {
			var result = new BoundScope();

			foreach (var function in BuiltInFunctions.GetAll()) {
				result.TryDefineFunction(function);
			}

			return result;
		}

		public static DiagnosticResult<BoundProgram> BindProgram(BoundGlobalScope globalScope) {
			var functionBodies = new Dictionary<FunctionSymbol, BoundBlock>();
			var diagnostics = new DiagnosticContainer();
			var parentScope = CreateParentScopes(globalScope);

			foreach (var function in globalScope.Functions) {
				if (globalScope.FunctionBodies.TryGetValue(function, out var functionDeclaration)) {
					var functionBinder = new Binder(parentScope, function);
					var bodyBind = functionBinder.Bind(functionDeclaration.Body);

					if (functionBinder.Diagnostics.Any())
						diagnostics.AddDiagnostics(functionBinder.Diagnostics);

					if (!(bodyBind is BoundStatement body))
						continue;

					functionBodies.Add(function, Lowerer.Lower(body));
				}
			}

			var program = new BoundProgram(functionBodies, globalScope.Root);
			return new DiagnosticResult<BoundProgram>(diagnostics, program);
		}

		private BoundNode Error(Diagnostic diagnostic, bool addToDiagnostics = true) {
			if (addToDiagnostics)
				Diagnostics.AddDiagnostic(diagnostic);

			return new BoundError(diagnostic);
		}

		private DiagnosticResult<BoundBlock> BindCompilationUnit(CompilationUnitSyntax compilationUnit) {
			if (compilationUnit == null)
				return new DiagnosticResult<BoundBlock>(Diagnostics, null);

			foreach (var functionDef in compilationUnit.Statements.OfType<FunctionDeclarationSyntax>()) {
				BindFunctionDeclaration(functionDef);
			}

			var statements = ImmutableArray.CreateBuilder<BoundStatement>();

			foreach (var statementSyntax in compilationUnit.Statements) {
				if (!(statementSyntax is FunctionDeclarationSyntax)) {
					var statementBind = Bind(statementSyntax);

					if (!(statementBind is BoundStatement statement))
						continue;

					statements.Add(statement);
				}
			}

			return new DiagnosticResult<BoundBlock>(Diagnostics, new BoundBlock(statements.ToImmutable()));
		}

		private BoundNode Bind(SyntaxNode syntax) {
			switch (syntax.Type) {
				case SyntaxType.Literal:
					return BindLiteral((LiteralSyntax)syntax);
				case SyntaxType.UnaryExpression:
					return BindUnaryExpression((UnaryExpressionSyntax)syntax);
				case SyntaxType.BinaryExpression:
					return BindBinaryExpression((BinaryExpressionSyntax)syntax);
				case SyntaxType.VariableDeclaration:
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
				case SyntaxType.FunctionCall:
					return BindFunctionCall((FunctionCallSyntax)syntax);
				case SyntaxType.GlobalStatement:
					return BindGlobalStatement((GlobalStatementSyntax)syntax);
				case SyntaxType.Break:
					return BindBreakStatement((BreakSyntax)syntax);
				case SyntaxType.Continue:
					return BindContinueStatement((ContinueSyntax)syntax);
				default: throw new Exception($"Encountered unhandled syntax {syntax.Type}");
			}
		}

		private BoundNode BindContinueStatement(ContinueSyntax syntax) {
			if (breakContinueLabels.Count == 0)
				return Error(Diagnostic.ReportInvalidContinueStatement(syntax.Location.Line, syntax.Location.Column, syntax.Span));

			return new BoundBranchStatement(breakContinueLabels.Peek().Item2);
		}

		private BoundNode BindBreakStatement(BreakSyntax syntax) {
			if (breakContinueLabels.Count == 0)
				return Error(Diagnostic.ReportInvalidBreakStatement(syntax.Location.Line, syntax.Location.Column, syntax.Span));

			return new BoundBranchStatement(breakContinueLabels.Peek().Item1);
		}

		private BoundNode BindGlobalStatement(GlobalStatementSyntax syntax) {
			return Bind(syntax.Statement);
		}

		private BoundNode BindFunctionCall(FunctionCallSyntax syntax) {
			string callName = syntax.Identifier.Token.Text;

			if (!scope.TryLookupFunction(callName, out var symbol))
				return Error(Diagnostic.ReportUndefinedFunction(syntax.Identifier.Location.Line, syntax.Identifier.Location.Column, syntax.Identifier.Span));

			if (syntax.Parameters.Count != symbol.Parameters.Length) {
				int syntaxCount = syntax.Parameters.Count;
				int requiredCount = symbol.Parameters.Length;

				return Error(Diagnostic.ReportFunctionCountMismatch(syntax.Identifier.Location.Line, syntax.Identifier.Location.Column, syntaxCount, requiredCount, syntax.Parameters.Span));
			}

			var expressions = ImmutableArray.CreateBuilder<BoundExpression>();

			if (syntax.Parameters.Count > 0) {
				for (int index = 0; index < syntax.Parameters.Count; index++) {
					var paramSyntax = syntax.Parameters[index];
					var requiredType = symbol.Parameters[index].ValueType;
					var paramVisit = Bind(paramSyntax);

					if (!(paramVisit is BoundExpression parameter))
						return paramVisit;

					if (parameter.ValueType != requiredType && !TypeConversionSymbol.TryFind(parameter.ValueType, requiredType, out _))
						return Error(Diagnostic.ReportInvalidParameterType(paramSyntax.Location.Line, paramSyntax.Location.Column, parameter.ValueType, requiredType, paramSyntax.Span));

					expressions.Add(parameter);
				}
			}

			return new BoundFunctionCall(symbol, expressions.ToImmutable());
		}

		private BoundNode BindBlock(BlockSyntax syntax) {
			var statements = ImmutableArray.CreateBuilder<BoundStatement>();

			foreach (var statSyntax in syntax.Statements) {
				var bound = Bind(statSyntax);

				if (!(bound is BoundStatement statement))
					return bound;

				statements.Add(statement);
			}

			return new BoundBlock(statements.ToImmutable());
		}

		private BoundNode BindError(ErrorSyntax syntax) {
			return Error(syntax.Diagnostic, false);
		}

		private (LabelSymbol, LabelSymbol) CreateLoopLabels() {
			var breakLabel = new LabelSymbol($"Break{currentBreakContinueNo}");
			var continueLabel = new LabelSymbol($"Continue{currentBreakContinueNo}");

			return (breakLabel, continueLabel);
		}

		private BoundNode BindForLoop(ForLoopSyntax syntax) {
			var assignVisit = Bind(syntax.Assignment);
			var conditionVisit = Bind(syntax.Condition);
			var stepVisit = Bind(syntax.Step);

			var labels = CreateLoopLabels();
			breakContinueLabels.Push(labels);

			scope = new BoundScope(scope);
			var bodyVisit = Bind(syntax.Body);
			scope = scope.Parent;

			if (!(assignVisit is BoundStatement assignment))
				return assignVisit;

			if (!(conditionVisit is BoundExpression condition))
				return conditionVisit;

			if (!(stepVisit is BoundExpression step))
				return stepVisit;

			if (!(bodyVisit is BoundStatement body))
				return bodyVisit;

			if (condition.ValueType != TypeSymbol.Boolean) {
				var prev = new TextSpan(syntax.ForToken.Span.Start, syntax.LeftParenToken.Span.End);
				var next = new TextSpan(syntax.Comma1.Span.Start, syntax.Comma1.Span.End);

				return Error(Diagnostic.ReportInvalidType(syntax.Condition.Location.Line, syntax.Condition.Location.Column, prev, syntax.Condition.Span, next, TypeSymbol.Boolean));
			}

			var bindRes = new BoundForStatement(assignment, condition, step, body, labels.Item1, labels.Item2);

			breakContinueLabels.Pop();
			return bindRes;
		}

		private BoundNode BindWhileLoop(WhileLoopSyntax syntax) {
			var conditionVisit = Bind(syntax.Condition);

			var labels = CreateLoopLabels();
			breakContinueLabels.Push(labels);

			scope = new BoundScope(scope);
			var bodyVisit = Bind(syntax.Body);
			scope = scope.Parent;

			if (!(conditionVisit is BoundExpression condition))
				return conditionVisit;

			if (!(bodyVisit is BoundStatement body))
				return bodyVisit;

			if (condition.ValueType != TypeSymbol.Boolean) {
				var prev = new TextSpan(syntax.WhileToken.Span.Start, syntax.LeftParenToken.Span.End);
				var next = new TextSpan(syntax.RightParenToken.Span.Start, syntax.RightParenToken.Span.End);

				return Error(Diagnostic.ReportInvalidType(syntax.Condition.Location.Line, syntax.Condition.Location.Column, prev, syntax.Condition.Span, next, TypeSymbol.Boolean));
			}

			var bindRes = new BoundWhileStatement(condition, body, labels.Item1, labels.Item2);

			breakContinueLabels.Pop();
			return bindRes;
		}

		private BoundNode BindIfStatement(IfStatementSyntax syntax) {
			var conditionVisit = Bind(syntax.Condition);

			scope = new BoundScope(scope);
			var trueBranchVisit = Bind(syntax.TrueBranch);
			scope = scope.Parent;

			scope = new BoundScope(scope);
			var falseBranchVisit = syntax.FalseBranch == null ? null : Bind(syntax.FalseBranch);
			scope = scope.Parent;

			if (!(conditionVisit is BoundExpression boundCondition))
				return conditionVisit;

			if (!(trueBranchVisit is BoundStatement boundTrueBr))
				return trueBranchVisit;

			if (falseBranchVisit != null && !(falseBranchVisit is BoundStatement))
				return falseBranchVisit;

			var boundFalseBr = falseBranchVisit == null ? null : (BoundStatement)falseBranchVisit;

			if (boundCondition.ValueType != TypeSymbol.Boolean) {
				var prev = new TextSpan(syntax.IfToken.Span.Start, syntax.LeftParenToken.Span.End);
				var next = new TextSpan(syntax.RightParenToken.Span.Start, syntax.RightParenToken.Span.End);

				return Error(Diagnostic.ReportInvalidType(syntax.Condition.Location.Line, syntax.Condition.Location.Column, prev, syntax.Condition.Span, next, TypeSymbol.Boolean));
			}

			return new BoundIfStatement(boundCondition, boundTrueBr, boundFalseBr);
		}

		private BoundNode BindExpressionStatement(ExpressionStatementSyntax syntax) {
			var expressionVisit = Bind(syntax.Expression);

			if (!(expressionVisit is BoundExpression expression))
				return expressionVisit;

			return new BoundExpressionStatement(expression);
		}

		private BoundNode BindAssignmentExpression(AssignmentExpressionSyntax syntax) {
			if (syntax.Definition != null) {
				return Error(Diagnostic.ReportInvalidAssignmentTypeDef(syntax.Definition.Location.Line, syntax.Definition.Location.Column, syntax.IdentifierToken.Span, syntax.Definition.Span));
			}

			string identifierText = syntax.IdentifierToken.Token.Text;
			var boundExpression = Bind(syntax.Expression);
			var prev = new TextSpan(syntax.IdentifierToken.Span.Start, syntax.OperatorToken.Span.End);

			if (!(boundExpression is BoundExpression expression))
				return boundExpression;

			if (expression.ValueType == TypeSymbol.Void)
				return Error(Diagnostic.ReportVoidType(syntax.Expression.Location.Line, syntax.Expression.Location.Column, prev, syntax.Expression.Span));

			if (!scope.TryLookupVariable(identifierText, out var variable))
				return Error(Diagnostic.ReportUndefinedVariable(syntax.IdentifierToken.Location.Line, syntax.IdentifierToken.Location.Column, syntax.IdentifierToken.Span));

			if (variable.ValueType != expression.ValueType && !TypeConversionSymbol.TryFind(expression.ValueType, variable.ValueType, out _))
				return Error(Diagnostic.ReportCannotCast(syntax.Expression.Location.Line, syntax.Expression.Location.Column, prev, syntax.Expression.Span, variable.ValueType, expression.ValueType));


			return new BoundAssignmentExpression(variable, expression);
		}

		private BoundNode BindVariableExpression(VariableSyntax syntax) {
			string varaibleName = syntax.IdentifierToken.Token.Text;

			if (!scope.TryLookupVariable(varaibleName, out var variable))
				return Error(Diagnostic.ReportUndefinedVariable(syntax.IdentifierToken.Location.Line, syntax.IdentifierToken.Location.Column, syntax.IdentifierToken.Span));

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
					return Error(Diagnostic.ReportUnknownDeclKeyword(syntax.KeywordToken.Location.Line, syntax.KeywordToken.Location.Column, syntax.KeywordToken.Span));
			}

			if (syntax.Initialiser != null) {
				var initialiserBind = Bind(syntax.Initialiser);

				if (!(initialiserBind is BoundExpression init))
					return initialiserBind;

				var line = syntax.Initialiser.Location.Line;
				var column = syntax.Initialiser.Location.Column;
				var prev = new TextSpan(syntax.KeywordToken.Span.Start, syntax.OperatorToken.Span.End);

				if (init.ValueType == TypeSymbol.Void)
					return Error(Diagnostic.ReportVoidType(line, column, prev, syntax.Initialiser.Span));

				initialiser = init;
			}

			if (syntax.Definition != null) {
				type = TypeSymbol.FromString(syntax.Definition.NameToken.ToString());

				if (type == null)
					return Error(Diagnostic.ReportUnknownTypeKeyword(syntax.Definition.Location.Line, syntax.Definition.Location.Column, syntax.Definition.DelimeterToken.Span, syntax.Definition.NameToken.Span));

				if (type == TypeSymbol.Void) {
					var line = syntax.Definition.NameToken.Location.Line;
					var column = syntax.Definition.NameToken.Location.Column;
					var prev = new TextSpan(syntax.KeywordToken.Span.Start, syntax.Definition.DelimeterToken.Span.End);

					return Error(Diagnostic.ReportVoidType(line, column, prev, syntax.Definition.NameToken.Span));
				}
			}

			if (type != null && initialiser != null && initialiser.ValueType != type && !TypeConversionSymbol.TryFind(initialiser.ValueType, type, out _)) {
				var prev = new TextSpan(syntax.KeywordToken.Span.Start, syntax.OperatorToken.Span.End);
				return Error(Diagnostic.ReportCannotCast(syntax.Definition.Location.Line, syntax.Definition.Location.Column, prev, syntax.Initialiser.Span, initialiser.ValueType, type));
			}

			if (initialiser != null && initialiser.ValueType == TypeSymbol.Void)
				return Error(Diagnostic.ReportVoidType(syntax.Initialiser.Location.Line, syntax.Initialiser.Location.Column, syntax.Definition.DelimeterToken.Span, syntax.Definition.NameToken.Span));

			type = type ?? initialiser.ValueType;
			var variable = new VariableSymbol(identifierText, isreadOnly, type);

			if (!scope.TryDefineVariable(variable))
				return Error(Diagnostic.ReportCannotRedefine(syntax.IdentifierToken.Location.Line, syntax.IdentifierToken.Location.Column, syntax.IdentifierToken.Span));

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
				return Error(Diagnostic.ReportInvalidOperator(syntax.OpToken.Location.Line, syntax.OpToken.Location.Column, syntax.LeftSyntax.Span, syntax.OpToken.Span, syntax.RightSyntax.Span, left.ValueType, right.ValueType));

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
				return Error(Diagnostic.ReportInvalidOperator(opToken.Line, opToken.Column, syntax.OpToken.Span, syntax.Expression.Span, boundSubExpression.ValueType));

			return new BoundUnaryExpression(op, boundSubExpression);
		}

		private BoundNode BindLiteral(LiteralSyntax syntax) {
			var literalText = syntax.LiteralToken.Token.Text;

			if (int.TryParse(literalText, out int intVal))
				return new BoundLiteral(intVal, TypeSymbol.Integer);

			if (double.TryParse(literalText, out double doubleVal))
				return new BoundLiteral(doubleVal, TypeSymbol.Double);

			if (bool.TryParse(literalText, out bool boolVal))
				return new BoundLiteral(boolVal, TypeSymbol.Boolean);

			var stringText = syntax.ToString();
			var text = stringText.Substring(1, stringText.Length - 2);
			return new BoundLiteral(text, TypeSymbol.String);
		}

		public BoundNode BindFunctionDeclaration(FunctionDeclarationSyntax syntax) {
			string funcName = syntax.Identifier.ToString();
			var parameters = ImmutableArray.CreateBuilder<ParameterSymbol>();
			var returnType = TypeSymbol.FromString(syntax.ReturnType.NameToken.ToString());

			foreach (var parameter in syntax.Parameters.Parameters) {
				string parameterName = parameter.Identifier.ToString();
				var typeBind = TypeSymbol.FromString(parameter.Definition.NameToken.ToString());

				int line = parameter.Definition.NameToken.Location.Line;
				int column = parameter.Definition.NameToken.Location.Column;
				var span = parameter.Definition.NameToken.Span;

				if (typeBind == null)
					return Error(Diagnostic.ReportInvalidParameterDefinition(line, column, span));

				if (typeBind == TypeSymbol.Void)
					return Error(Diagnostic.ReportVoidType(line, column, span));

				parameters.Add(new ParameterSymbol(parameterName, typeBind));
			}

			if (returnType == null)
				return Error(Diagnostic.ReportInvalidReturnType(syntax.ReturnType.Location.Line, syntax.ReturnType.Location.Column, syntax.ReturnType.Span));

			var functionSymbol = new FunctionSymbol(funcName, parameters.ToImmutable(), returnType);

			if (!scope.TryDefineFunction(functionSymbol))
				return Error(Diagnostic.ReportCannotRedefineFunction(syntax.Identifier.Location.Line, syntax.Identifier.Location.Column, syntax.Identifier.Span));

			functionBodies.Add(functionSymbol, syntax);
			return null;
		}
	}
}
