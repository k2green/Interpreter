using InterpreterLib.Binding.Lowering;
using InterpreterLib.Binding.Tree;
using InterpreterLib.Binding.Tree.Expressions;
using InterpreterLib.Binding.Tree.Statements;
using InterpreterLib.Syntax.Tree;
using InterpreterLib.Syntax.Tree.Expressions;
using InterpreterLib.Syntax.Tree.Global;
using InterpreterLib.Syntax.Tree.Statements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Immutable;
using InterpreterLib.Syntax;
using InterpreterLib.Symbols.Binding;
using InterpreterLib.Symbols.Types;

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
			functionBodies = new Dictionary<FunctionSymbol, FunctionDeclarationSyntax>();
			Function = function;
			breakContinueLabels = new Stack<(LabelSymbol, LabelSymbol)>();
			currentBreakContinueNo = 0;

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
					var body = functionBinder.BindStatement(functionDeclaration.Body, true);

					var builder = ImmutableArray.CreateBuilder<BoundStatement>();
					builder.Add(body);
					builder.Add(new BoundLabel(function.EndLabel));

					if (functionBinder.Diagnostics.Any()) {
						diagnostics.AddDiagnostics(functionBinder.Diagnostics);
						continue;
					}

					functionBodies.Add(function, Lowerer.Lower(new BoundBlock(builder.ToImmutable())));
				}
			}

			var program = new BoundProgram(functionBodies, globalScope.Root);
			return new DiagnosticResult<BoundProgram>(diagnostics, program);
		}

		private BoundStatement ErrorStatement(Diagnostic diagnostic) {
			Diagnostics.AddDiagnostic(diagnostic);

			return new BoundErrorStatement(diagnostic);
		}

		private BoundExpression ErrorExpression(Diagnostic diagnostic) {
			Diagnostics.AddDiagnostic(diagnostic);

			return new BoundErrorExpression(diagnostic);
		}

		private (LabelSymbol, LabelSymbol) CreateLoopLabels() {
			var breakLabel = new LabelSymbol($"Break{currentBreakContinueNo}");
			var continueLabel = new LabelSymbol($"Continue{currentBreakContinueNo}");

			return (breakLabel, continueLabel);
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
					var statementBind = BindGlobalStatement(statementSyntax);

					if (!(statementBind is BoundStatement statement))
						continue;

					statements.Add(statement);
				}
			}

			return new DiagnosticResult<BoundBlock>(Diagnostics, new BoundBlock(statements.ToImmutable()));
		}

		private BoundExpression BindExpression(ExpressionSyntax expression) {
			switch (expression.Type) {
				case SyntaxType.Literal:
					return BindLiteral((LiteralSyntax)expression);
				case SyntaxType.Variable:
					return BindVariableExpression((VariableSyntax)expression);
				case SyntaxType.UnaryExpression:
					return BindUnaryExpression((UnaryExpressionSyntax)expression);
				case SyntaxType.BinaryExpression:
					return BindBinaryExpression((BinaryExpressionSyntax)expression);
				case SyntaxType.Assignment:
					return BindAssignmentExpression((AssignmentExpressionSyntax)expression);
				case SyntaxType.FunctionCall:
					return BindFunctionCall((FunctionCallSyntax)expression);
				default: throw new Exception($"Encountered unhandled expression syntax {expression.Type}");
			}
		}

		private BoundExpression BindLiteral(LiteralSyntax syntax) {
			return new BoundLiteral(syntax.Value);
		}

		private BoundExpression BindVariableExpression(VariableSyntax syntax) {
			string varaibleName = syntax.IdentifierToken.Token.Text;

			if (!scope.TryLookupVariable(varaibleName, out var variable))
				return ErrorExpression(Diagnostic.ReportUndefinedVariable(syntax.IdentifierToken.Location, syntax.IdentifierToken.Span));

			return new BoundVariableExpression(variable);
		}

		private BoundExpression BindUnaryExpression(UnaryExpressionSyntax syntax) {
			var opToken = syntax.OpToken;
			var opText = opToken.ToString();
			var boundSubExpression = BindExpression(syntax.Expression);

			var op = UnaryOperator.Bind(opText, boundSubExpression.ValueType);

			if (op == null) {
				var diagnostic = Diagnostic.ReportInvalidOperator(opToken.Location, syntax.OpToken.Span, syntax.Expression.Span, boundSubExpression.ValueType);
				return ErrorExpression(diagnostic);
			}

			return new BoundUnaryExpression(op, boundSubExpression);
		}

		private BoundExpression BindBinaryExpression(BinaryExpressionSyntax syntax) {
			var opToken = syntax.OpToken.Token;
			var opText = opToken.Text;

			var boundLeft = BindExpression(syntax.LeftSyntax);
			var boundRight = BindExpression(syntax.RightSyntax);

			var op = BinaryOperator.Bind(opText, boundLeft.ValueType, boundRight.ValueType);

			if (op == null) {
				var diagnostic = Diagnostic.ReportInvalidOperator(syntax.OpToken.Location, syntax.LeftSyntax.Span, syntax.OpToken.Span, syntax.RightSyntax.Span, boundLeft.ValueType, boundRight.ValueType);
				return ErrorExpression(diagnostic);
			}

			return new BoundBinaryExpression(boundLeft, op, boundRight);
		}

		private BoundExpression BindAssignmentExpression(AssignmentExpressionSyntax syntax) {
			if (syntax.Definition != null) {
				var diagnostic = Diagnostic.ReportInvalidAssignmentTypeDef(syntax.Definition.Location, syntax.IdentifierToken.Span, syntax.Definition.Span);
				return ErrorExpression(diagnostic);
			}

			string identifierText = syntax.IdentifierToken.Token.Text;
			var expression = BindExpression(syntax.Expression);
			var prev = new TextSpan(syntax.IdentifierToken.Span.Start, syntax.OperatorToken.Span.End);

			if (expression.ValueType == ValueTypeSymbol.Void) {
				var diagnostic = Diagnostic.ReportVoidType(syntax.Expression.Location, prev, syntax.Expression.Span);
				return ErrorExpression(diagnostic);
			}

			if (!scope.TryLookupVariable(identifierText, out var variable)) {
				var diagnostic = Diagnostic.ReportUndefinedVariable(syntax.IdentifierToken.Location, syntax.IdentifierToken.Span);
				return ErrorExpression(diagnostic);
			}

			if (variable.ValueType != expression.ValueType && !TypeConversionSymbol.TryFind(expression.ValueType, variable.ValueType, out _)) {
				var diagnostic = Diagnostic.ReportCannotCast(syntax.Expression.Location, prev, syntax.Expression.Span, variable.ValueType, expression.ValueType);
				return ErrorExpression(diagnostic);
			}

			return new BoundAssignmentExpression(variable, expression);
		}

		private BoundExpression BindFunctionCall(FunctionCallSyntax syntax) {
			string callName = syntax.Identifier.Token.Text;

			if (!scope.TryLookupFunction(callName, out var symbol)) {
				var diagnostic = Diagnostic.ReportUndefinedFunction(syntax.Identifier.Location, syntax.Identifier.Span);
				return ErrorExpression(diagnostic);
			}

			if (syntax.Parameters.Count != symbol.Parameters.Length) {
				int syntaxCount = syntax.Parameters.Count;
				int requiredCount = symbol.Parameters.Length;

				var diagnostic = Diagnostic.ReportFunctionCountMismatch(syntax.Identifier.Location, syntaxCount, requiredCount, syntax.Parameters.Span);
				return ErrorExpression(diagnostic);
			}

			var expressions = ImmutableArray.CreateBuilder<BoundExpression>();

			if (syntax.Parameters.Count > 0) {
				for (int index = 0; index < syntax.Parameters.Count; index++) {
					var paramSyntax = syntax.Parameters[index];
					var requiredType = symbol.Parameters[index].ValueType;
					var parameter = BindExpression(paramSyntax);

					if (parameter.ValueType != requiredType && !TypeConversionSymbol.TryFind(parameter.ValueType, requiredType, out _)) {
						var diagnostic = Diagnostic.ReportInvalidParameterType(paramSyntax.Location, parameter.ValueType, requiredType, paramSyntax.Span);
						return ErrorExpression(diagnostic);
					}

					expressions.Add(parameter);
				}
			}

			return new BoundFunctionCall(symbol, expressions.ToImmutable());
		}

		private BoundStatement BindGlobalStatement(GlobalSyntax syntax) {
			switch (syntax.Type) {
				case SyntaxType.GlobalStatement:
					return BindGlobalStatement((GlobalStatementSyntax)syntax);
				default: throw new Exception($"Encountered unhandled syntax {syntax.Type}");
			}
		}

		private BoundStatement BindGlobalStatement(GlobalStatementSyntax syntax) {
			return BindStatement(syntax.Statement);
		}

		private BoundStatement BindStatement(StatementSyntax syntax, bool isFunctionBody = false, bool isLast = false) {
			switch (syntax.Type) {
				case SyntaxType.VariableDeclaration:
					return BindDeclarationStatement((VariableDeclarationSyntax)syntax);
				case SyntaxType.Expression:
					return BindExpressionStatement((ExpressionStatementSyntax)syntax);
				case SyntaxType.IfStatement:
					return BindIfStatement((IfStatementSyntax)syntax, isLast);
				case SyntaxType.WhileLoop:
					return BindWhileLoop((WhileLoopSyntax)syntax);
				case SyntaxType.ForLoop:
					return BindForLoop((ForLoopSyntax)syntax);
				case SyntaxType.Block:
					return BindBlock((BlockSyntax)syntax, isFunctionBody);
				case SyntaxType.Break:
					return BindBreakStatement((BreakSyntax)syntax);
				case SyntaxType.Continue:
					return BindContinueStatement((ContinueSyntax)syntax);
				case SyntaxType.Return:
					return BindReturnStatement((ReturnSyntax)syntax);
				default: throw new Exception($"Encountered unhandled syntax {syntax.Type}");
			}
		}

		private BoundStatement BindReturnStatement(ReturnSyntax syntax) {
			if (Function != null) {
				if (syntax.Expression != null) {
					var expression = BindExpression(syntax.Expression);

					if (expression.Type == NodeType.Error)
						return new BoundExpressionStatement(expression);

					if (Function.ReturnType == ValueTypeSymbol.Void) {
						var exprDiag = Diagnostic.ReportInvalidReturnExpression(syntax.Location, syntax.Span);
						return ErrorStatement(exprDiag);
					}

					if (expression.ValueType == Function.ReturnType) {
						var bodyBuilder = ImmutableArray.CreateBuilder<BoundStatement>();

						bodyBuilder.Add(new BoundExpressionStatement(expression));
						bodyBuilder.Add(new BoundBranchStatement(Function.EndLabel));

						return new BoundBlock(bodyBuilder.ToImmutable());
					} else {
						var exprDiag = Diagnostic.ReportInvalidReturnExpressionType(syntax.Location, syntax.Span, expression.ValueType, Function.ReturnType);
						return ErrorStatement(exprDiag);
					}
				} else {
					return new BoundBranchStatement(Function.EndLabel);
				}
			}

			var diagnostic = Diagnostic.ReportInvalidReturnStatement(syntax.Location, syntax.Span);
			return ErrorStatement(diagnostic);
		}

		private BoundStatement BindDeclarationStatement(VariableDeclarationSyntax syntax) {
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
					var diagnostic = Diagnostic.ReportUnknownDeclKeyword(syntax.KeywordToken.Location, syntax.KeywordToken.Span);
					return ErrorStatement(diagnostic);
			}

			if (syntax.Initialiser != null) {
				initialiser = BindExpression(syntax.Initialiser);

				if (initialiser.Type == NodeType.Error)
					return new BoundExpressionStatement(initialiser);

				var prev = new TextSpan(syntax.KeywordToken.Span.Start, syntax.OperatorToken.Span.End);

				if (initialiser.ValueType == ValueTypeSymbol.Void)
					return ErrorStatement(Diagnostic.ReportVoidType(syntax.Initialiser.Location, prev, syntax.Initialiser.Span));
			}

			if (syntax.Definition != null) {
				type = TypeSymbol.FromString(syntax.Definition.NameToken.ToString());

				if (type == null;) {
					var diagnostic = Diagnostic.ReportUnknownTypeKeyword(syntax.Definition.Location, syntax.Definition.DelimeterToken.Span, syntax.Definition.NameToken.Span);
					return ErrorStatement(diagnostic);
				}

				if (type == ValueTypeSymbol.Void) {
					var location = syntax.Definition.NameToken.Location;
					var prev = new TextSpan(syntax.KeywordToken.Span.Start, syntax.Definition.DelimeterToken.Span.End);

					return ErrorStatement(Diagnostic.ReportVoidType(location, prev, syntax.Definition.NameToken.Span));
				}
			}

			if (type != null && initialiser != null && initialiser.ValueType != type && !TypeConversionSymbol.TryFind(initialiser.ValueType, type, out _)) {
				var prev = new TextSpan(syntax.KeywordToken.Span.Start, syntax.OperatorToken.Span.End);
				return ErrorStatement(Diagnostic.ReportCannotCast(syntax.Definition.Location, prev, syntax.Initialiser.Span, initialiser.ValueType, type));
			}

			if (initialiser != null && initialiser.ValueType == ValueTypeSymbol.Void)
				return ErrorStatement(Diagnostic.ReportVoidType(syntax.Initialiser.Location, syntax.Definition.DelimeterToken.Span, syntax.Definition.NameToken.Span));

			type = type ?? initialiser.ValueType;
			var variable = new VariableSymbol(identifierText, isreadOnly, type);

			if (!scope.TryDefineVariable(variable))
				return ErrorStatement(Diagnostic.ReportCannotRedefine(syntax.IdentifierToken.Location, syntax.IdentifierToken.Span));

			return new BoundVariableDeclarationStatement(variable, initialiser);
		}

		private BoundStatement BindExpressionStatement(ExpressionStatementSyntax syntax) {
			var expression = BindExpression(syntax.Expression);

			if (expression.Type == NodeType.Error)
				return new BoundExpressionStatement(expression);

			return new BoundExpressionStatement(expression);
		}

		private BoundStatement BindIfStatement(IfStatementSyntax syntax, bool isLastFunctionStatement = false) {
			var boundCondition = BindExpression(syntax.Condition);

			scope = new BoundScope(scope);
			var boundTrueBr = BindStatement(syntax.TrueBranch, isLastFunctionStatement, isLastFunctionStatement);
			scope = scope.Parent;

			scope = new BoundScope(scope);
			var boundFalseBr = syntax.FalseBranch == null ? null : BindStatement(syntax.FalseBranch, isLastFunctionStatement, isLastFunctionStatement);
			scope = scope.Parent;

			if (boundCondition.Type == NodeType.Error)
				return new BoundExpressionStatement(boundCondition);

			if (boundTrueBr.Type == NodeType.Error)
				return boundTrueBr;

			if (boundFalseBr != null && boundFalseBr.Type == NodeType.Error)
				return boundFalseBr;

			if (Function != null && isLastFunctionStatement) {
				if (boundFalseBr == null) {
					return ErrorStatement(Diagnostic.ReportNoReturn(syntax.Location, syntax.Span));
				} else {
					if (!(syntax.TrueBranch is ReturnSyntax) && (boundTrueBr is BoundExpressionStatement trueExpression && trueExpression.Expression.ValueType != Function.ReturnType)) {
						return ErrorStatement(Diagnostic.ReportNoReturn(syntax.Location, syntax.TrueBranch.Span));
					}

					if (!(syntax.FalseBranch is ReturnSyntax) && boundFalseBr is BoundExpressionStatement falseExpression && falseExpression.Expression.ValueType != Function.ReturnType) {
						return ErrorStatement(Diagnostic.ReportNoReturn(syntax.Location, syntax.FalseBranch.Span));
					}
				}
			}

			if (boundCondition.ValueType != ValueTypeSymbol.Boolean) {
				var prev = new TextSpan(syntax.IfToken.Span.Start, syntax.LeftParenToken.Span.End);
				var next = new TextSpan(syntax.RightParenToken.Span.Start, syntax.RightParenToken.Span.End);

				var diagnostic = Diagnostic.ReportInvalidType(syntax.Condition.Location, prev, syntax.Condition.Span, next, ValueTypeSymbol.Boolean);
				return ErrorStatement(diagnostic);
			}

			return new BoundIfStatement(boundCondition, boundTrueBr, boundFalseBr);
		}

		private BoundStatement BindWhileLoop(WhileLoopSyntax syntax) {
			var condition = BindExpression(syntax.Condition);

			var labels = CreateLoopLabels();
			breakContinueLabels.Push(labels);

			scope = new BoundScope(scope);
			var body = BindStatement(syntax.Body);
			scope = scope.Parent;

			if (condition.Type == NodeType.Error)
				return new BoundExpressionStatement(condition);

			if (body.Type == NodeType.Error)
				return body;

			if (condition.ValueType != ValueTypeSymbol.Boolean) {
				var prev = new TextSpan(syntax.WhileToken.Span.Start, syntax.LeftParenToken.Span.End);
				var next = new TextSpan(syntax.RightParenToken.Span.Start, syntax.RightParenToken.Span.End);

				var diagnostic = Diagnostic.ReportInvalidType(syntax.Condition.Location, prev, syntax.Condition.Span, next, ValueTypeSymbol.Boolean);
				return ErrorStatement(diagnostic);
			}

			var bindRes = new BoundWhileStatement(condition, body, labels.Item1, labels.Item2);

			breakContinueLabels.Pop();
			return bindRes;
		}

		private BoundStatement BindForLoop(ForLoopSyntax syntax) {
			var assignment = BindStatement(syntax.Assignment);
			var condition = BindExpression(syntax.Condition);
			var step = BindExpression(syntax.Step);

			var labels = CreateLoopLabels();
			breakContinueLabels.Push(labels);

			scope = new BoundScope(scope);
			var body = BindStatement(syntax.Body);
			scope = scope.Parent;

			if (assignment.Type == NodeType.Error)
				return assignment;

			if (condition.Type == NodeType.Error)
				return new BoundExpressionStatement(condition);

			if (step.Type == NodeType.Error)
				return new BoundExpressionStatement(step);

			if (body.Type == NodeType.Error)
				return body;

			if (condition.ValueType != ValueTypeSymbol.Boolean) {
				var prev = new TextSpan(syntax.ForToken.Span.Start, syntax.LeftParenToken.Span.End);
				var next = new TextSpan(syntax.Comma1.Span.Start, syntax.Comma1.Span.End);

				var diagnostic = Diagnostic.ReportInvalidType(syntax.Condition.Location, prev, syntax.Condition.Span, next, ValueTypeSymbol.Boolean);
				return ErrorStatement(diagnostic);
			}

			var bindRes = new BoundForStatement(assignment, condition, step, body, labels.Item1, labels.Item2);

			breakContinueLabels.Pop();
			return bindRes;
		}

		private BoundStatement BindBlock(BlockSyntax syntax, bool isFunctionBody = false) {
			var statements = ImmutableArray.CreateBuilder<BoundStatement>();
			bool hasReturn = false;

			var paramCount = syntax.Statements.Length;
			for (var index = 0; index < paramCount; index++) {
				var statSyntax = syntax.Statements[index];
				BoundStatement statement;

				if (isFunctionBody && Function.ReturnType != ValueTypeSymbol.Void && index == paramCount - 1 && statSyntax is IfStatementSyntax ifSyntax) {
					statement = BindIfStatement(ifSyntax, true);

					hasReturn = statement.Type != NodeType.Error;
				} else {
					statement = BindStatement(statSyntax);
				}

				if (statSyntax is ReturnSyntax)
					hasReturn = true;

				if (statement.Type == NodeType.Error)
					return statement;

				statements.Add(statement);
			}

			var immutable = statements.ToImmutable();

			if (isFunctionBody && Function.ReturnType != ValueTypeSymbol.Void) {
				if (immutable.Length > 0 && immutable.Last() is BoundExpressionStatement lastStatement && lastStatement.Expression.ValueType == Function.ReturnType)
					hasReturn = true;

				if (!hasReturn) {
					var diagnostic = Diagnostic.ReportNoReturn(syntax.Location, syntax.Span);
					return ErrorStatement(diagnostic);
				}
			}

			return new BoundBlock(immutable);
		}

		private BoundStatement BindBreakStatement(BreakSyntax syntax) {
			if (breakContinueLabels.Count == 0)
				return ErrorStatement(Diagnostic.ReportInvalidBreakOrContinueStatement(syntax.Location, syntax.Span, syntax.BreakToken.ToString()));

			return new BoundBranchStatement(breakContinueLabels.Peek().Item1);
		}

		private BoundStatement BindContinueStatement(ContinueSyntax syntax) {
			if (breakContinueLabels.Count == 0)
				return ErrorStatement(Diagnostic.ReportInvalidBreakOrContinueStatement(syntax.Location, syntax.Span, syntax.ContinueToken.ToString()));

			return new BoundBranchStatement(breakContinueLabels.Peek().Item2);
		}

		public BoundStatement BindFunctionDeclaration(FunctionDeclarationSyntax syntax) {
			string funcName = syntax.Identifier.ToString();
			var parameters = ImmutableArray.CreateBuilder<ParameterSymbol>();
			var returnType = ValueTypeSymbol.FromString(syntax.ReturnType.NameToken.ToString());

			foreach (var parameter in syntax.Parameters) {
				string parameterName = parameter.Identifier.ToString();
				var typeBind = ValueTypeSymbol.FromString(parameter.Definition.NameToken.ToString());

				int line = parameter.Definition.NameToken.Location.Line;
				int column = parameter.Definition.NameToken.Location.Column;
				var span = parameter.Definition.NameToken.Span;

				if (typeBind == null)
					return ErrorStatement(Diagnostic.ReportInvalidParameterDefinition(line, column, span));

				if (typeBind == ValueTypeSymbol.Void)
					return ErrorStatement(Diagnostic.ReportVoidType(parameter.Definition.NameToken.Location, span));

				parameters.Add(new ParameterSymbol(parameterName, typeBind));
			}

			if (returnType == null)
				return ErrorStatement(Diagnostic.ReportInvalidReturnType(syntax.ReturnType.Location, syntax.ReturnType.Span));

			var functionSymbol = new FunctionSymbol(funcName, parameters.ToImmutable(), returnType);

			if (!scope.TryDefineFunction(functionSymbol))
				return ErrorStatement(Diagnostic.ReportCannotRedefineFunction(syntax.Identifier.Location.Line, syntax.Identifier.Location.Column, syntax.Identifier.Span));

			if (functionBodies.ContainsKey(functionSymbol))
				functionBodies.Remove(functionSymbol);

			functionBodies.Add(functionSymbol, syntax);
			return null;
		}
	}
}
