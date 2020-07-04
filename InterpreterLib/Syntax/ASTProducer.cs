using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using InterpreterLib.Syntax.Tree;
using InterpreterLib.Syntax.Tree.Expressions;
using InterpreterLib.Syntax.Tree.Statements;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace InterpreterLib.Syntax {
	internal class ASTProducer : GLangBaseVisitor<SyntaxNode> {

		private DiagnosticContainer diagnostics;
		private List<FunctionDeclarationSyntax> functionDeclarations;

		public ASTProducer() {
			diagnostics = new DiagnosticContainer();
			functionDeclarations = new List<FunctionDeclarationSyntax>();
		}

		private bool OnlyOne(IEnumerable<bool> conditions) => conditions.Count(b => b) == 1;
		private bool OnlyOne(params bool[] conditions) => OnlyOne((IEnumerable<bool>)conditions);

		private TokenSyntax Token(IToken token) => new TokenSyntax(token);

		private SyntaxNode Error(Diagnostic diagnostic) {
			diagnostics.AddDiagnostic(diagnostic);

			return new ErrorSyntax(diagnostic);
		}

		public static DiagnosticResult<SyntaxNode> CreateAST(IParseTree tree) {
			ASTProducer producer = new ASTProducer();

			var syntaxTree = producer.Visit(tree);

			return new DiagnosticResult<SyntaxNode>(producer.diagnostics, syntaxTree);
		}

		public override SyntaxNode VisitLiteral([NotNull] GLangParser.LiteralContext context) {
			// Check for each token type in a literal
			bool hasInt = context.INTEGER() != null;
			bool hasDouble = context.DOUBLE() != null;
			bool hasBool = context.BOOLEAN() != null;
			bool hasIdentifier = context.IDENTIFIER() != null;
			bool hasString = context.STRING() != null;

			// Returns an error if there isn'n exactly one token
			if (!(OnlyOne(hasInt, hasIdentifier, hasBool, hasString, hasDouble)))
				return Error(Diagnostic.ReportInvalidLiteral(context.Start.Line, context.Start.Column, context.GetText()));

			// Return a literal of variable Syntax depending on which token exists
			if (hasDouble)
				return new LiteralSyntax(new TokenSyntax(context.DOUBLE().Symbol));

			if (hasInt)
				return new LiteralSyntax(new TokenSyntax(context.INTEGER().Symbol));

			if (hasBool)
				return new LiteralSyntax(new TokenSyntax(context.BOOLEAN().Symbol));

			if (hasIdentifier)
				return new VariableSyntax(new TokenSyntax(context.IDENTIFIER().Symbol));

			if (hasString)
				return new LiteralSyntax(new TokenSyntax(context.STRING().Symbol));

			// As a last resort, returns an error.
			return Error(Diagnostic.ReportInvalidLiteral(context.Start.Line, context.Start.Column, context.GetText()));
		}

		public override SyntaxNode VisitUnaryExpression([NotNull] GLangParser.UnaryExpressionContext context) {

			bool hasAtom = context.atom != null;
			bool hasUnaryOperation = context.op != null && context.unaryExpression() != null;
			bool hasExpression = context.L_PARENTHESIS() != null && context.binaryExpression() != null && context.R_PARENTHESIS() != null;

			if (!OnlyOne(hasAtom, hasUnaryOperation, hasExpression))
				return Error(Diagnostic.ReportInvalidUnaryExpression(context.Start.Line, context.Start.Column, context.GetText()));

			// Visit the atom if it exists
			if (hasAtom)
				return Visit(context.atom);

			if (hasExpression)
				return Visit(context.binaryExpression());


			// Visit the binary expression
			var visit = Visit(context.unaryExpression());

			// Ensures the visit is an expression
			if (!(visit is ExpressionSyntax))
				return Error(Diagnostic.ReportInvalidUnaryExpression(context.Start.Line, context.Start.Column, context.GetText()));

			return new UnaryExpressionSyntax(Token(context.op), (ExpressionSyntax)visit);
		}

		public override SyntaxNode VisitBinaryExpression([NotNull] GLangParser.BinaryExpressionContext context) {
			bool hasAtom = context.atom != null && !string.IsNullOrEmpty(context.atom.GetText());
			bool hasLeft = context.left != null && !string.IsNullOrEmpty(context.left.GetText());
			bool hasOperator = context.op != null && !string.IsNullOrEmpty(context.op.Text);
			bool hasRight = context.right != null && !string.IsNullOrEmpty(context.right.GetText());

			// Ensures that either the context has both a left and right expression with an operator, or that it has an atom.
			if (hasOperator && (!hasLeft || !hasRight || hasAtom))
				return Error(Diagnostic.ReportInvalidBinaryExpression(context.Start.Line, context.Start.Column, context.GetText()));

			if (hasAtom && (hasOperator || hasLeft || hasRight))
				return Error(Diagnostic.ReportInvalidBinaryExpression(context.Start.Line, context.Start.Column, context.GetText()));

			// Visit the atom if it exists
			if (hasAtom)
				return Visit(context.unaryExpression());

			// Visit the sub-expression
			var visitLeft = Visit(context.left);
			var visitRight = Visit(context.right);

			// Ensures the visis are expressions
			if (!(visitLeft is ExpressionSyntax && visitRight is ExpressionSyntax))
				return Error(Diagnostic.ReportInvalidBinaryExpression(context.Start.Line, context.Start.Column, context.GetText()));

			return new BinaryExpressionSyntax((ExpressionSyntax)visitLeft, Token(context.op), (ExpressionSyntax)visitRight);
		}

		public override SyntaxNode VisitTypeDefinition([NotNull] GLangParser.TypeDefinitionContext context) {
			var delimeterCtx = context.TYPE_DELIMETER();
			var nameCtx = context.TYPE_NAME();

			if (delimeterCtx == null || !delimeterCtx.GetText().Equals(":"))
				return Error(Diagnostic.ReportMissingTypeDelimeter(context.Start.Line, context.Start.Column, context.GetText()));

			if (delimeterCtx == null)
				return Error(Diagnostic.ReportMissingTypeName(context.Start.Line, context.Start.Column, context.GetText()));

			return new TypeDefinitionSyntax(new TokenSyntax(delimeterCtx.Symbol), new TokenSyntax(nameCtx.Symbol));
		}

		public override SyntaxNode VisitAssignmentExpression([NotNull] GLangParser.AssignmentExpressionContext context) {
			var identifierCtx = context.IDENTIFIER();
			var typeDefCtx = context.typeDefinition();
			var operatorCtx = context.ASSIGNMENT_OPERATOR();
			var expressionCtx = context.binaryExpression();

			if (identifierCtx == null)
				return Error(Diagnostic.ReportMissingIdentifier(context.Start.Line, context.Start.Column, context.GetText()));

			if (operatorCtx == null)
				return Error(Diagnostic.ReportMissingOperator(context.Start.Line, context.Start.Column, context.GetText()));

			if (expressionCtx == null)
				return Error(Diagnostic.ReportMissingExpression(context.Start.Line, context.Start.Column, context.GetText()));

			TypeDefinitionSyntax typeDef = null;
			if (typeDefCtx != null) {
				var visit = Visit(typeDefCtx);

				if (!(visit is TypeDefinitionSyntax))
					return Error(Diagnostic.ReportInvalidTypeDefinition(typeDefCtx.Start.Line, typeDefCtx.Start.Column, typeDefCtx.GetText()));

				typeDef = (TypeDefinitionSyntax)visit;
			}

			var visitExpr = Visit(expressionCtx);

			if (!(visitExpr is ExpressionSyntax))
				return Error(Diagnostic.ReportInvalidAssignmentOperand(expressionCtx.Start.Line, expressionCtx.Start.Column, expressionCtx.GetText()));

			return new AssignmentExpressionSyntax(Token(identifierCtx.Symbol), typeDef, Token(operatorCtx.Symbol), (ExpressionSyntax)visitExpr);
		}

		public override SyntaxNode VisitVariableDeclarationStatement([NotNull] GLangParser.VariableDeclarationStatementContext context) {
			var keywordCtx = context.DECL_VARIABLE();
			var identifierCtx = context.definedIdentifier();
			var assignmentCtx = context.assignmentExpression();

			bool hasDirectDecl = identifierCtx != null;
			bool isAssignmentDecl = assignmentCtx != null;

			if (keywordCtx == null || !(keywordCtx.Symbol.Text.Equals("var") || keywordCtx.Symbol.Text.Equals("var")))
				return Error(Diagnostic.ReportInvalidDeclarationKeyword(context.Start.Line, context.Start.Column, context.GetText()));

			if (!OnlyOne(hasDirectDecl, isAssignmentDecl))
				return Error(Diagnostic.ReportMalformedDeclaration(keywordCtx.Symbol.Line, keywordCtx.Symbol.Column, context.GetText()));

			if (hasDirectDecl) {
				var defVisit = Visit(context.definedIdentifier());

				if (!(defVisit is TypedIdentifierSyntax))
					return Error(Diagnostic.ReportMalformedDeclaration(keywordCtx.Symbol.Line, keywordCtx.Symbol.Column, context.GetText()));

				var def = (TypedIdentifierSyntax)defVisit;
				return new VariableDeclarationSyntax(Token(keywordCtx.Symbol), def.Identifier, def.Definition, null, null);
			} else {
				var assignVisit = Visit(context.assignmentExpression());

				if (!(assignVisit is AssignmentExpressionSyntax))
					return Error(Diagnostic.ReportMalformedDeclaration(keywordCtx.Symbol.Line, keywordCtx.Symbol.Column, context.GetText()));

				var assignExpr = (AssignmentExpressionSyntax)assignVisit;
				return new VariableDeclarationSyntax(Token(keywordCtx.Symbol), assignExpr.IdentifierToken, assignExpr.Definition, assignExpr.OperatorToken, assignExpr.Expression);
			}
		}

		public override SyntaxNode VisitExpressionStatement([NotNull] GLangParser.ExpressionStatementContext context) {
			var assignmentCtx = context.assignmentExpression();
			var expressionCtx = context.binaryExpression();

			if (!OnlyOne(assignmentCtx != null, expressionCtx != null))
				return Error(Diagnostic.ReportInvalidExpressionStatement(context.Start.Line, context.Start.Column, context.GetText()));

			SyntaxNode visit;

			if (assignmentCtx != null)
				visit = Visit(assignmentCtx);
			else
				visit = Visit(expressionCtx);

			if (!(visit is ExpressionSyntax))
				return Error(Diagnostic.ReportInvalidExpressionStatement(context.Start.Line, context.Start.Column, context.GetText()));

			return new ExpressionStatementSyntax((ExpressionSyntax)visit);
		}

		public override SyntaxNode VisitDeclerationOrAssign([NotNull] GLangParser.DeclerationOrAssignContext context) {
			if (context.variableDeclarationStatement() != null)
				return Visit(context.variableDeclarationStatement());

			if (context.assignmentExpression() != null) {
				var visit = Visit(context.assignmentExpression());

				if (!(visit is ExpressionSyntax))
					return Error(Diagnostic.ReportInvalidForAssignment(context.Start.Line, context.Start.Column, context.GetText()));

				return new ExpressionStatementSyntax((ExpressionSyntax)visit);
			}

			return Error(Diagnostic.ReportInvalidForAssignment(context.Start.Line, context.Start.Column, context.GetText()));
		}

		public override SyntaxNode VisitIfStatement([NotNull] GLangParser.IfStatementContext context) {
			var ifKeywCtx = context.IF();
			var lParenCtx = context.L_PARENTHESIS();
			var conditionCtx = context.binaryExpression();
			var rParenCtx = context.R_PARENTHESIS();
			var trueCtx = context.trueBranch;
			var elseCtx = context.ELSE();
			var falseCtx = context.falseBranch;

			if (ifKeywCtx == null || conditionCtx == null || lParenCtx == null || rParenCtx == null || trueCtx == null)
				return Error(Diagnostic.ReportInvalidIfStatement(context.Start.Line, context.Start.Column, context.GetText()));

			if (!ifKeywCtx.GetText().Equals("if") || !lParenCtx.GetText().Equals("(") || !rParenCtx.GetText().Equals(")"))
				return Error(Diagnostic.ReportInvalidIfStatement(context.Start.Line, context.Start.Column, context.GetText()));

			if (elseCtx == null ^ falseCtx == null)
				return Error(Diagnostic.ReportInvalidIfStatement(context.Start.Line, context.Start.Column, context.GetText()));

			if (elseCtx != null && !elseCtx.GetText().Equals("else"))
				return Error(Diagnostic.ReportInvalidIfStatement(context.Start.Line, context.Start.Column, context.GetText()));

			var condVisit = Visit(conditionCtx);
			var trueVisit = Visit(trueCtx);

			if (!(condVisit is ExpressionSyntax && trueVisit is StatementSyntax))
				return Error(Diagnostic.ReportInvalidIfStatement(context.Start.Line, context.Start.Column, context.GetText()));

			TokenSyntax elseToken = null;
			StatementSyntax falseStatement = null;

			if (elseCtx != null) {
				elseToken = Token(elseCtx.Symbol);
				var falseVisit = Visit(falseCtx);

				if (!(falseVisit is StatementSyntax))
					return Error(Diagnostic.ReportInvalidIfStatement(context.Start.Line, context.Start.Column, context.GetText()));

				elseToken = Token(elseCtx.Symbol);
				falseStatement = (StatementSyntax)falseVisit;
			}

			return new IfStatementSyntax(Token(ifKeywCtx.Symbol), Token(lParenCtx.Symbol), (ExpressionSyntax)condVisit, Token(rParenCtx.Symbol), (StatementSyntax)trueVisit, elseToken, falseStatement);
		}

		public override SyntaxNode VisitWhileStatement([NotNull] GLangParser.WhileStatementContext context) {
			var whileKeywCtx = context.WHILE();
			var lParenCtx = context.L_PARENTHESIS();
			var conditionCtx = context.binaryExpression();
			var rParenCtx = context.R_PARENTHESIS();
			var bodyCtx = context.statement();

			if (whileKeywCtx == null || conditionCtx == null || lParenCtx == null || rParenCtx == null)
				return Error(Diagnostic.ReportInvalidForStatement(context.Start.Line, context.Start.Column, context.GetText()));

			if (!whileKeywCtx.GetText().Equals("while") || !lParenCtx.GetText().Equals("(") || !rParenCtx.GetText().Equals(")"))
				return Error(Diagnostic.ReportInvalidIfStatement(context.Start.Line, context.Start.Column, context.GetText()));

			var condVisit = Visit(conditionCtx);
			var bodyVisit = Visit(bodyCtx);

			if (!(condVisit is ExpressionSyntax && bodyVisit is StatementSyntax))
				return Error(Diagnostic.ReportInvalidForStatement(context.Start.Line, context.Start.Column, context.GetText()));

			return new WhileLoopSyntax(Token(whileKeywCtx.Symbol), Token(lParenCtx.Symbol), (ExpressionSyntax)condVisit, Token(rParenCtx.Symbol), (StatementSyntax)bodyVisit);
		}

		public override SyntaxNode VisitForStatement([NotNull] GLangParser.ForStatementContext context) {
			var forKeywCtx = context.FOR();
			var lParenCtx = context.L_PARENTHESIS();
			var assignmentCtx = context.declerationOrAssign();
			var commasCtx = context.COMMA();
			var conditionCtx = context.binaryExpression();
			var stepCtx = context.assignmentExpression();
			var rParenCtx = context.R_PARENTHESIS();
			var bodyCtx = context.statement();

			if (forKeywCtx == null || lParenCtx == null || assignmentCtx == null || commasCtx == null
				|| conditionCtx == null || stepCtx == null || rParenCtx == null || bodyCtx == null)
				return Error(Diagnostic.ReportInvalidForStatement(context.Start.Line, context.Start.Column, context.GetText()));

			if (!forKeywCtx.GetText().Equals("for") || !lParenCtx.GetText().Equals("(") || !rParenCtx.GetText().Equals(")"))
				return Error(Diagnostic.ReportInvalidForStatement(context.Start.Line, context.Start.Column, context.GetText()));

			var assignVisit = Visit(context.declerationOrAssign());
			var condVisit = Visit(context.binaryExpression());
			var stepVisit = Visit(context.assignmentExpression());
			var bodyVisit = Visit(context.statement());

			if (!(assignVisit is StatementSyntax && condVisit is ExpressionSyntax && stepVisit is ExpressionSyntax && bodyVisit is StatementSyntax))
				return Error(Diagnostic.ReportInvalidForStatement(context.Start.Line, context.Start.Column, context.GetText()));

			return new ForLoopSyntax(
				Token(forKeywCtx.Symbol),
				Token(lParenCtx.Symbol),
				(StatementSyntax)assignVisit, Token(commasCtx[0].Symbol), (ExpressionSyntax)condVisit, Token(commasCtx[1].Symbol), (ExpressionSyntax)stepVisit,
				Token(rParenCtx.Symbol),
				(StatementSyntax)bodyVisit);
		}

		public override SyntaxNode VisitBlock([NotNull] GLangParser.BlockContext context) {
			var leftBraceCtx = context.L_BRACE();
			var statementsCtx = context.statement();
			var rightBraceCtx = context.R_BRACE();

			bool hasLeftBrace = leftBraceCtx != null && leftBraceCtx.Symbol.Text.Equals("{");
			bool hasRightBrace = rightBraceCtx != null && rightBraceCtx.Symbol.Text.Equals("}");

			if (!hasLeftBrace || !hasRightBrace || statementsCtx == null)
				return Error(Diagnostic.ReportInvalidBody(context.Start.Line, context.Start.Column, context.GetText()));

			List<StatementSyntax> statements = new List<StatementSyntax>();

			foreach (var ctx in statementsCtx) {
				var visit = Visit(ctx);

				if (visit is StatementSyntax)
					statements.Add((StatementSyntax)visit);
				else
					return Error(Diagnostic.ReportFailedVisit(ctx.Start.Line, ctx.Start.Column, ctx.GetText()));
			}

			return new BlockSyntax(Token(leftBraceCtx.Symbol), statements, Token(rightBraceCtx.Symbol));
		}

		public override SyntaxNode VisitFunctionCall([NotNull] GLangParser.FunctionCallContext context) {
			var funcIdentifierCtx = context.funcName;
			var leftParenCtx = context.L_PARENTHESIS();
			var rightParenCtx = context.R_PARENTHESIS();

			if (funcIdentifierCtx == null || leftParenCtx == null || rightParenCtx == null || !leftParenCtx.GetText().Equals("(") || !rightParenCtx.GetText().Equals(")"))
				return Error(Diagnostic.ReportInvalidFunctionCall(context.Start.Line, context.Start.Column, context.GetText()));

			var firstCtx = context.seperatedExpression();
			var lastCtx = context.last;

			if (firstCtx != null && firstCtx.Length > 0 && lastCtx == null)
				return Error(Diagnostic.ReportInvalidCallParameters(context.Start.Line, context.Start.Column, context.GetText()));

			List<SyntaxNode> nodes = new List<SyntaxNode>();

			if (firstCtx != null && firstCtx.Length > 0 && lastCtx != null) {
				foreach (var ctx in firstCtx) {
					var expressionCtx = ctx.binaryExpression();
					var commaCtx = ctx.COMMA();

					if (commaCtx == null || !commaCtx.GetText().Equals(","))
						return Error(Diagnostic.ReportMissingComma(expressionCtx.Start.Line, expressionCtx.Start.Column, expressionCtx.GetText()));

					var visit = Visit(expressionCtx);

					if (!(visit is ExpressionSyntax expressionSyntax))
						return Error(Diagnostic.ReportInvalidCallParameter(expressionCtx.Start.Line, expressionCtx.Start.Column, expressionCtx.GetText()));

					nodes.Add(expressionSyntax);
					nodes.Add(Token(commaCtx.Symbol));
				}
			}

			if (lastCtx != null) {
				var lastVisit = Visit(lastCtx);

				if (!(lastVisit is ExpressionSyntax lastExpressionSyntax))
					return Error(Diagnostic.ReportInvalidCallParameter(lastCtx.Start.Line, lastCtx.Start.Column, lastCtx.GetText()));

				nodes.Add(lastExpressionSyntax);
			}

			return new FunctionCallSyntax(Token(funcIdentifierCtx), Token(leftParenCtx.Symbol), new SeperatedSyntaxList<ExpressionSyntax>(nodes), Token(rightParenCtx.Symbol));
		}

		public override SyntaxNode VisitErrorNode(IErrorNode node) {
			return Error(Diagnostic.ReportSyntaxError(node.Symbol.Line, node.Symbol.Column, node.GetText()));
		}

		public override SyntaxNode VisitStatement([NotNull] GLangParser.StatementContext context) {
			if (context.functionDefinition() != null)
				return Visit(context.functionDefinition());

			if (context.functionCall() != null)
				return Visit(context.functionCall());

			if (context.forStatement() != null)
				return Visit(context.forStatement());

			if (context.whileStatement() != null)
				return Visit(context.whileStatement());

			if (context.ifStatement() != null)
				return Visit(context.ifStatement());

			if (context.block() != null)
				return Visit(context.block());

			if (context.variableDeclarationStatement() != null)
				return Visit(context.variableDeclarationStatement());

			if (context.expressionStatement() != null)
				return Visit(context.expressionStatement());

			return Error(Diagnostic.ReportFailedVisit(context.Start.Line, context.Start.Column, context.GetText()));
		}

		public override SyntaxNode VisitDefinedIdentifier([NotNull] GLangParser.DefinedIdentifierContext context) {
			var identifierCtx = context.IDENTIFIER();
			var typeDefCtx = context.typeDefinition();

			if (identifierCtx == null || typeDefCtx == null)
				return Error(Diagnostic.ReportInvalidTypedIdentifier(context.Start.Line, context.Start.Column, context.GetText()));

			var typeDefVisit = Visit(typeDefCtx);

			if (!(typeDefVisit is TypeDefinitionSyntax))
				return Error(Diagnostic.ReportFailedVisit(context.Start.Line, context.Start.Column, context.GetText()));

			return new TypedIdentifierSyntax(Token(identifierCtx.Symbol), (TypeDefinitionSyntax)typeDefVisit);
		}

		public override SyntaxNode VisitParametersDefinition([NotNull] GLangParser.ParametersDefinitionContext context) {
			var leftParenCtx = context.L_PARENTHESIS();
			var rightParenCtx = context.R_PARENTHESIS();

			var firstCtx = context.seperatedDefinedIdentifier();
			var lastCtx = context.last;

			if (leftParenCtx == null || rightParenCtx == null || !leftParenCtx.GetText().Equals("(") || !rightParenCtx.GetText().Equals(")"))
				return Error(Diagnostic.ReportInvalidParameterDefinition(context.Start.Line, context.Start.Column, context.GetText()));

			if (firstCtx != null && firstCtx.Length > 0 && lastCtx == null)
				return Error(Diagnostic.ReportInvalidParameterDefinition(context.Start.Line, context.Start.Column, context.GetText()));

			List<SyntaxNode> parameters = new List<SyntaxNode>();

			if (firstCtx != null && firstCtx.Length > 0) {
				foreach (var ctx in firstCtx) {
					if (ctx.definedIdentifier() == null || ctx.COMMA() == null || !ctx.COMMA().GetText().Equals(","))
						return Error(Diagnostic.ReportInvalidParameterDefinition(context.Start.Line, context.Start.Column, context.GetText()));

					var visit = Visit(ctx.definedIdentifier());

					if (!(visit is TypedIdentifierSyntax identSyntax))
						return Error(Diagnostic.ReportInvalidParameterDefinition(context.Start.Line, context.Start.Column, context.GetText()));

					parameters.Add(identSyntax);
					parameters.Add(Token(ctx.COMMA().Symbol));
				}
			}

			if (lastCtx != null) { 

				var lastVisit = Visit(lastCtx);

				if (!(lastVisit is TypedIdentifierSyntax lastIdentSyntax))
					return Error(Diagnostic.ReportInvalidParameterDefinition(context.Start.Line, context.Start.Column, context.GetText()));

				parameters.Add(lastIdentSyntax);

				return new ParameterDefinitionSyntax(Token(leftParenCtx.Symbol), new SeperatedSyntaxList<TypedIdentifierSyntax>(parameters), Token(rightParenCtx.Symbol));
			}

			return new ParameterDefinitionSyntax(Token(leftParenCtx.Symbol), new SeperatedSyntaxList<TypedIdentifierSyntax>(parameters), Token(rightParenCtx.Symbol));
		}

		public override SyntaxNode VisitFunctionDefinition([NotNull] GLangParser.FunctionDefinitionContext context) {
			var keywCtx = context.FUNCTION();
			var identCtx = context.IDENTIFIER();
			var paramCtx = context.parametersDefinition();
			var typeDefCtx = context.typeDefinition();
			var bodyCtx = context.statement();

			if (keywCtx == null || paramCtx == null || typeDefCtx == null || bodyCtx == null || !keywCtx.GetText().Equals("function"))
				return Error(Diagnostic.ReportInvalidFunctionDef(context.Start.Line, context.Start.Column, context.GetText()));

			var paramVisit = Visit(paramCtx);
			var typeDefVisit = Visit(typeDefCtx);
			var bodyVisit = Visit(bodyCtx);

			if (!(paramVisit is ParameterDefinitionSyntax && typeDefVisit is TypeDefinitionSyntax && bodyVisit is StatementSyntax))
				return Error(Diagnostic.ReportInvalidFunctionDef(context.Start.Line, context.Start.Column, context.GetText()));

			var keywToken = Token(keywCtx.Symbol);
			var identToken = identCtx == null ? null : Token(identCtx.Symbol);
			var parameters = (ParameterDefinitionSyntax)paramVisit;
			var typeDef = (TypeDefinitionSyntax)typeDefVisit;
			var body = (StatementSyntax)bodyVisit;

			var declSyntax = new FunctionDeclarationSyntax(keywToken, identToken, parameters, typeDef, body);

			functionDeclarations.Add(declSyntax);
			return declSyntax;
		}
	}
}
