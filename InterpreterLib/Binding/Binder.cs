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
using InterpreterLib.Syntax.Tree.TypeDescriptions;

namespace InterpreterLib.Binding {
	internal sealed class Binder : GLangBaseVisitor<BoundNode> {
		public DiagnosticContainer Diagnostics { get; }

		private BoundScope scope;
		private FunctionSymbol Function;

		private Stack<(LabelSymbol, LabelSymbol)> breakContinueLabels;

		private int currentBreakContinueNo;

		private Binder(BoundScope parent, FunctionSymbol function = null) {
			scope = new BoundScope(parent);
			Diagnostics = new DiagnosticContainer();
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
				globScope = new BoundGlobalScope(prev, binder.scope.GetVariables(), binder.scope.GetFunctions(), null);
				return new DiagnosticResult<BoundGlobalScope>(binder.Diagnostics, globScope);
			}

			var lowered = Lowerer.Lower(res.Value);
			globScope = new BoundGlobalScope(prev, binder.scope.GetVariables(), binder.scope.GetFunctions(), lowered);
			return new DiagnosticResult<BoundGlobalScope>(binder.Diagnostics, globScope);
		}

		public static BoundScope CreateParentScopes(BoundGlobalScope previous) {
			var list = new List<FunctionSymbol>();
			return CreateParentScopes(previous, ref list);
		}

		public static BoundScope CreateParentScopes(BoundGlobalScope previous, ref List<FunctionSymbol> parentFunctions) {
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

				foreach (var function in previous.Functions) {
					current.TryDefineFunction(function);
					parentFunctions.Add(function);
				}
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

			var allFunctions = new List<FunctionSymbol>();
			var parentScope = CreateParentScopes(globalScope, ref allFunctions);

			foreach (var function in allFunctions) {
				if (function.FuncSyntax != null && !functionBodies.ContainsKey(function)) {
					var subDiagnostic = BindFunction(function, new BoundScope(parentScope), ref functionBodies);

					if (subDiagnostic != null)
						diagnostics.AddDiagnostics(subDiagnostic);
				}
			}

			var program = new BoundProgram(functionBodies, globalScope.Root);
			return new DiagnosticResult<BoundProgram>(diagnostics, program);
		}

		private static DiagnosticContainer BindFunction(FunctionSymbol function, BoundScope scope, ref Dictionary<FunctionSymbol, BoundBlock> bodies) {
			var functionBinder = new Binder(scope, function);
			var body = functionBinder.BindStatement(function.FuncSyntax.Body);

			if (!function.ReturnType.Equals(ValueTypeSymbol.Void) && !StatementHasReturn(body, function)) {
				functionBinder.Diagnostics.AddDiagnostic(Diagnostic.ReportNoReturn(function.FuncSyntax.Body.Location, function.FuncSyntax.Body.Span));
				return functionBinder.Diagnostics;
			}

			var builder = ImmutableArray.CreateBuilder<BoundStatement>();
			builder.Add(body);
			builder.Add(new BoundLabel(function.EndLabel));

			if (functionBinder.Diagnostics.Any())
				return functionBinder.Diagnostics;

			bodies.Add(function, Lowerer.Lower(new BoundBlock(builder.ToImmutable()), function.EndLabel));
			return null;
		}

		private static bool StatementHasReturn(BoundStatement statement, FunctionSymbol function) {
			switch (statement.Type) {
				case NodeType.Return:
					return true;
				case NodeType.If:
					return HasReturn((BoundIfStatement)statement, function);
				case NodeType.Block:
					return HasReturn((BoundBlock)statement, function);
				case NodeType.Expression:
					return HasReturn((BoundExpressionStatement)statement, function);
				default:
					return false;
			}
		}

		private static bool HasReturn(BoundIfStatement statement, FunctionSymbol function) {
			if (statement.FalseBranch == null) return false;

			return StatementHasReturn(statement.TrueBranch, function) && StatementHasReturn(statement.FalseBranch, function);
		}

		private static bool HasReturn(BoundBlock statement, FunctionSymbol function) {
			if (statement.Statements.Length <= 0) return false;

			return StatementHasReturn(statement.Statements.Last(), function);
		}

		private static bool HasReturn(BoundExpressionStatement statement, FunctionSymbol function) {
			if (statement.Expression.ValueType.Equals(function.ReturnType)) {
				statement.MarkForRewrite();
				return true;
			}

			return false;
		}

		private BoundStatement ErrorStatement(Diagnostic diagnostic) {
			Diagnostics.AddDiagnostic(diagnostic);

			return new BoundErrorStatement(diagnostic);
		}

		private BoundExpression ErrorExpression(Diagnostic diagnostic) {
			Diagnostics.AddDiagnostic(diagnostic);

			return new BoundErrorExpression(diagnostic);
		}

		private BoundExpression Convert(BoundErrorStatement error) => ErrorExpression(error.Diagnostic);
		private BoundStatement Convert(BoundErrorExpression error) => ErrorStatement(error.Diagnostic);

		private (LabelSymbol, LabelSymbol) CreateLoopLabels() {
			var breakLabel = new LabelSymbol($"Break{currentBreakContinueNo}");
			var continueLabel = new LabelSymbol($"Continue{currentBreakContinueNo}");
			currentBreakContinueNo++;

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

		private BoundExpression BindExpression(ExpressionSyntax expression, bool allowVoid = true) {
			var boundExpression = InternalBindExpression(expression);

			if (boundExpression.Type == NodeType.Error)
				return boundExpression;

			if (boundExpression.ValueType.Equals(ValueTypeSymbol.Void) && !allowVoid) {
				return ErrorExpression(Diagnostic.ReportVoidType(expression.Location, expression.Span));
			}

			return boundExpression;
		}

		private BoundExpression InternalBindExpression(ExpressionSyntax expression) {
			switch (expression.Type) {
				case SyntaxType.Literal:
					return BindLiteral((LiteralSyntax)expression);
				case SyntaxType.Variable:
					return BindVariableExpression((VariableSyntax)expression);
				case SyntaxType.Accessor:
					return BindAccessor((AccessorSyntax)expression);
				case SyntaxType.UnaryExpression:
					return BindUnaryExpression((UnaryExpressionSyntax)expression);
				case SyntaxType.BinaryExpression:
					return BindBinaryExpression((BinaryExpressionSyntax)expression);
				case SyntaxType.Assignment:
					return BindAssignmentExpression((AssignmentExpressionSyntax)expression);
				case SyntaxType.FunctionCall:
					return BindFunctionCall((FunctionCallSyntax)expression);
				case SyntaxType.Tuple:
					return BindTuple((TupleSyntax)expression);
				default: throw new Exception($"Encountered unhandled expression syntax {expression.Type}");
			}
		}

		private BoundExpression BindLiteral(LiteralSyntax syntax) {
			return new BoundLiteral(syntax.Value);
		}

		private BoundExpression BindVariableExpression(VariableSyntax syntax) {
			string indentifierText = syntax.IdentifierToken.Token.Text;

			if (scope.TryLookupVariable(indentifierText, out var variable))
				return new BoundVariableExpression(variable);
			else if (scope.TryLookupFunction(indentifierText, out var function))
				return new BoundFunctionPointer(function);

			return ErrorExpression(Diagnostic.ReportUndefinedVariable(syntax.IdentifierToken.Location, syntax.IdentifierToken.Span));
		}

		private BoundExpression BindAccessor(AccessorSyntax expression, bool isReadOnly = false, VariableSymbol readonlyVariable = null) {
			BoundExpression boundItem;
			BoundExpression boundIndex = null;

			if (expression.Item.Type == SyntaxType.VariableIndexer) {
				(boundItem, boundIndex) = BindIndexer((VariableIndexerSyntax)expression.Item);
			} else {
				boundItem = BindExpression(expression.Item);
			}

			if (boundItem.Type == NodeType.Error)
				return boundItem;
			else if (boundItem.Type == NodeType.FunctionCall)
				isReadOnly = true;

			if (boundIndex != null && boundIndex.Type == NodeType.Error)
				return boundItem;

			if (expression.IsLast) {
				if (boundItem.Type == NodeType.AssignmentExpression && isReadOnly) {
					var variable = readonlyVariable ?? ((BoundAssignmentExpression)boundItem).Identifier;
					return ErrorExpression(Diagnostic.ReportReadOnlyVariable(expression.Item.Location, expression.Item.Span, variable));
				}

				return new BoundAccessor(boundItem, boundIndex, null);
			} else {
				var currentScope = scope;

				if (boundItem.ValueType is AccessibleSymbol accessible) {
					scope = new BoundScope(scope);

					foreach (var variable in accessible.Variables)
						scope.TryDefineVariable(variable);
				}

				var boundRest = BindAccessor(expression.Rest, isReadOnly, readonlyVariable);

				scope = currentScope;

				if (boundRest.Type != NodeType.Accessor)
					return boundRest;

				return new BoundAccessor(boundItem, boundItem, (BoundAccessor)boundRest);
			}
		}

		private (BoundExpression, BoundExpression) BindIndexer(VariableIndexerSyntax indexerSyntax) {
			var itemExpression = BindExpression(indexerSyntax.Expression);
			var indexExpression = BindExpression(indexerSyntax.Expression);

			if (!indexExpression.ValueType.Equals(ValueTypeSymbol.Integer)) {
				var prev = new TextSpan(indexerSyntax.Item.Span.Start, indexerSyntax.LeftBracket.Span.End);
				var next = new TextSpan(indexerSyntax.RightBracket.Span.Start, indexerSyntax.RightBracket.Span.End);
				indexExpression = ErrorExpression(Diagnostic.ReportInvalidType(indexerSyntax.Expression.Location, prev, indexerSyntax.Expression.Span, next, ValueTypeSymbol.Integer));
			}

			return (itemExpression, indexExpression);
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
			string identifierText = syntax.IdentifierToken.Token.Text;
			var expression = BindExpression(syntax.Expression, false);
			var prev = new TextSpan(syntax.IdentifierToken.Span.Start, syntax.OperatorToken.Span.End);

			if (!scope.TryLookupVariable(identifierText, out var variable)) {
				var diagnostic = Diagnostic.ReportUndefinedVariable(syntax.IdentifierToken.Location, syntax.IdentifierToken.Span);
				return ErrorExpression(diagnostic);
			}

			if (variable.IsReadOnly) {
				return ErrorExpression(Diagnostic.ReportReadOnlyVariable(syntax.Location, syntax.Span, variable));
			}

			if (!variable.ValueType.Equals(expression.ValueType) && !TypeConversionSymbol.TryFind(expression.ValueType, variable.ValueType, out _)) {
				var diagnostic = Diagnostic.ReportCannotCast(syntax.Expression.Location, prev, syntax.Expression.Span, variable.ValueType, expression.ValueType);
				return ErrorExpression(diagnostic);
			}

			return new BoundAssignmentExpression(variable, expression);
		}

		private BoundExpression BindFunctionCall(FunctionCallSyntax syntax) {
			string callName = syntax.Identifier.Token.Text;

			ImmutableArray<TypeSymbol> parameters;
			FunctionSymbol functionSymbol = null;
			VariableSymbol<FunctionTypeSymbol> pointerSymbol = null;

			if (scope.TryLookupFunction(callName, out functionSymbol)) {
				parameters = functionSymbol.Parameters.Select(param => param.ValueType).ToImmutableArray();
			} else if (scope.TryLookupVariable(callName, out pointerSymbol)) {
				parameters = pointerSymbol.ValueType.ParamTypes;
			} else {
				var diagnostic = Diagnostic.ReportUndefinedFunction(syntax.Identifier.Location, syntax.Identifier.Span);
				return ErrorExpression(diagnostic);
			}

			if (syntax.Parameters.Count != parameters.Length) {
				int syntaxCount = syntax.Parameters.Count;
				int requiredCount = parameters.Length;

				var diagnostic = Diagnostic.ReportFunctionCountMismatch(syntax.Identifier.Location, syntaxCount, requiredCount, syntax.Parameters.Span);
				return ErrorExpression(diagnostic);
			}

			var expressions = ImmutableArray.CreateBuilder<BoundExpression>();

			if (syntax.Parameters.Count > 0) {
				for (int index = 0; index < syntax.Parameters.Count; index++) {
					var paramSyntax = syntax.Parameters[index];
					var requiredType = parameters[index];
					var parameter = BindExpression(paramSyntax);

					if (!parameter.ValueType.Equals(requiredType) && !TypeConversionSymbol.TryFind(parameter.ValueType, requiredType, out _)) {
						var diagnostic = Diagnostic.ReportInvalidParameterType(paramSyntax.Location, parameter.ValueType, requiredType, paramSyntax.Span);
						return ErrorExpression(diagnostic);
					}

					expressions.Add(parameter);
				}
			}

			return new BoundFunctionCall(functionSymbol, pointerSymbol, expressions.ToImmutable());
		}

		private BoundExpression BindTuple(TupleSyntax expression) {
			var expressionBuilder = ImmutableArray.CreateBuilder<BoundExpression>();

			foreach (var item in expression.Items) {
				var expr = BindExpression(item);

				if (expr.Type == NodeType.Error)
					return expr;

				expressionBuilder.Add(expr);
			}

			return new BoundTuple(expressionBuilder.ToImmutable());
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

		private BoundStatement BindStatement(StatementSyntax syntax) {
			switch (syntax.Type) {
				case SyntaxType.VariableDeclaration:
					return BindDeclarationStatement((VariableDeclarationSyntax)syntax);
				case SyntaxType.Expression:
					return BindExpressionStatement((ExpressionStatementSyntax)syntax);
				case SyntaxType.IfStatement:
					return BindIfStatement((IfStatementSyntax)syntax);
				case SyntaxType.WhileLoop:
					return BindWhileLoop((WhileLoopSyntax)syntax);
				case SyntaxType.ForLoop:
					return BindForLoop((ForLoopSyntax)syntax);
				case SyntaxType.Block:
					return BindBlock((BlockSyntax)syntax);
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
						return Convert((BoundErrorExpression)expression);

					if (Function.ReturnType.Equals(ValueTypeSymbol.Void)) {
						var exprDiag = Diagnostic.ReportInvalidReturnExpression(syntax.Location, syntax.Span);
						return ErrorStatement(exprDiag);
					}

					if (expression.ValueType.Equals(Function.ReturnType)) {
						return new BoundReturnStatement(expression, Function.EndLabel);
					} else {
						var exprDiag = Diagnostic.ReportInvalidReturnExpressionType(syntax.Location, syntax.Span, expression.ValueType, Function.ReturnType);
						return ErrorStatement(exprDiag);
					}
				} else {
					return new BoundReturnStatement(null, Function.EndLabel);
				}
			}

			var diagnostic = Diagnostic.ReportInvalidReturnStatement(syntax.Location, syntax.Span);
			return ErrorStatement(diagnostic);
		}

		private BoundStatement BindDeclarationStatement(VariableDeclarationSyntax syntax) {
			var declText = syntax.KeywordToken.Token.Text;

			string varName;
			bool isReadOnly;
			TypeSymbol type = null;
			BoundExpression initialiser = null;

			switch (declText) {
				case "var":
					isReadOnly = false;
					break;
				case "val":
					isReadOnly = true;
					break;

				default:
					var diagnostic = Diagnostic.ReportUnknownDeclKeyword(syntax.KeywordToken.Location, syntax.KeywordToken.Span);
					return ErrorStatement(diagnostic);
			}

			if (syntax.Identifier != null) {
				varName = syntax.Identifier.IdentifierName.ToString();
				type = BindTypeDescription(syntax.Identifier.Definition.TypeDescription);

				if (type == null)
					return null;

				if (type.Equals(ValueTypeSymbol.Void)) {
					return ErrorStatement(Diagnostic.ReportVoidType(syntax.Identifier.Definition.TypeDescription.Location, syntax.Identifier.Definition.TypeDescription.Span));
				}
			} else {
				varName = syntax.Initialiser.IdentifierToken.ToString();
				initialiser = BindExpression(syntax.Initialiser.Expression, false);

				if (initialiser.Type == NodeType.Error)
					return Convert((BoundErrorExpression)initialiser);

				type = initialiser.ValueType;
			}

			var variable = new VariableSymbol(varName, isReadOnly, type);

			if (!scope.TryDefineVariable(variable)) {
				return ErrorStatement(Diagnostic.ReportCannotRedefine(syntax.Location, syntax.Span));
			}

			return new BoundVariableDeclarationStatement(variable, initialiser);
		}

		private TypeSymbol BindTypeDescription(SyntaxNode syntax, bool isReadOnly = false) {
			switch (syntax.Type) {
				case SyntaxType.ValueType:
					return BindValueType((ValueTypeSyntax)syntax, isReadOnly);
				case SyntaxType.TupleType:
					return BindTupleType((TupleTypeSyntax)syntax, isReadOnly);
				default: return null;
			}
		}

		private TypeSymbol BindTupleType(TupleTypeSyntax syntax, bool isReadOnly) {
			var builder = ImmutableArray.CreateBuilder<TypeSymbol>();

			foreach (var type in syntax.Types) {
				var boundType = BindTypeDescription(type, isReadOnly);

				if (boundType == null)
					return null;

				builder.Add(boundType);
			}

			return new TupleSymbol(builder.ToImmutable());
		}

		private TypeSymbol BindValueType(ValueTypeSyntax syntax, bool isReadOnly) {
			return ValueTypeSymbol.FromString(syntax.TypeName.ToString());
		}

		private BoundStatement BindExpressionStatement(ExpressionStatementSyntax syntax) {
			var expression = BindExpression(syntax.Expression);

			if (expression.Type == NodeType.Error)
				return new BoundExpressionStatement(expression);

			return new BoundExpressionStatement(expression);
		}

		private BoundStatement BindIfStatement(IfStatementSyntax syntax) {
			var boundCondition = BindExpression(syntax.Condition);

			scope = new BoundScope(scope);
			var boundTrueBr = BindStatement(syntax.TrueBranch);
			scope = scope.Parent;

			scope = new BoundScope(scope);
			var boundFalseBr = syntax.FalseBranch == null ? null : BindStatement(syntax.FalseBranch);
			scope = scope.Parent;

			if (boundCondition.Type == NodeType.Error)
				return new BoundExpressionStatement(boundCondition);

			if (boundTrueBr.Type == NodeType.Error)
				return boundTrueBr;

			if (boundFalseBr != null && boundFalseBr.Type == NodeType.Error)
				return boundFalseBr;

			if (!boundCondition.ValueType.Equals(ValueTypeSymbol.Boolean)) {
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

		private BoundStatement BindBlock(BlockSyntax syntax) {
			var statements = ImmutableArray.CreateBuilder<BoundStatement>();

			var paramCount = syntax.Statements.Length;
			for (var index = 0; index < paramCount; index++) {
				var statSyntax = syntax.Statements[index];
				var statement = BindStatement(statSyntax);

				if (statement.Type == NodeType.Error)
					return statement;

				statements.Add(statement);

				if (statement.Type == NodeType.Return) {
					break;
				}
			}

			return new BoundBlock(statements.ToImmutable());
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
			var returnType = BindTypeDescription(syntax.ReturnType.TypeDescription);

			foreach (var parameter in syntax.Parameters) {
				string parameterName = parameter.IdentifierName.ToString();
				var typeBind = BindTypeDescription(parameter.Definition.TypeDescription);

				int line = parameter.Definition.TypeDescription.Location.Line;
				int column = parameter.Definition.TypeDescription.Location.Column;
				var span = parameter.Definition.TypeDescription.Span;

				if (typeBind == null)
					return ErrorStatement(Diagnostic.ReportInvalidParameterDefinition(line, column, span));

				if (typeBind.Equals(ValueTypeSymbol.Void))
					return ErrorStatement(Diagnostic.ReportVoidType(parameter.Definition.TypeDescription.Location, span));

				parameters.Add(new ParameterSymbol(parameterName, typeBind));
			}

			if (returnType == null)
				return ErrorStatement(Diagnostic.ReportInvalidReturnType(syntax.ReturnType.Location, syntax.ReturnType.Span));

			var functionSymbol = new FunctionSymbol(funcName, parameters.ToImmutable(), returnType, syntax);

			if (!scope.TryDefineFunction(functionSymbol))
				return ErrorStatement(Diagnostic.ReportCannotRedefineFunction(syntax.Identifier.Location, syntax.Identifier.Span));

			return null;
		}
	}
}
