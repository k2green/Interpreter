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
		private TextWriter writer;

		public ASTProducer(TextWriter output) {
			diagnostics = new DiagnosticContainer();
			writer = output;
		}

		private bool OnlyOne(IEnumerable<bool> conditions) => conditions.Count(b => b) == 1;
		private bool OnlyOne(params bool[] conditions) => OnlyOne((IEnumerable<bool>)conditions);

		private TokenSyntax Token(IToken token) => new TokenSyntax(token);

		private SyntaxNode Error(Diagnostic diagnostic) {
			diagnostics.AddDiagnostic(diagnostic);

			return new ErrorSyntax(diagnostic);
		}

		public override SyntaxNode VisitLiteral([NotNull] GLangParser.LiteralContext context) {
			bool hasInt = context.INTEGER() != null;
			bool hasBool = context.BOOLEAN() != null;
			bool hasIdentifier = context.IDENTIFIER() != null;

			if (!(OnlyOne(hasInt, hasIdentifier, hasBool)))
				return Error(Diagnostic.ReportInvalidLiteral(context.Start.Line, context.Start.Column, context.GetText()));

			if (hasInt)
				return new LiteralSyntax(new TokenSyntax(context.INTEGER().Symbol));

			if (hasBool)
				return new LiteralSyntax(new TokenSyntax(context.BOOLEAN().Symbol));

			if (hasIdentifier)
				return new VariableSyntax(new TokenSyntax(context.IDENTIFIER().Symbol));

			return Error(Diagnostic.ReportInvalidLiteral(context.Start.Line, context.Start.Column, context.GetText()));
		}

		public override SyntaxNode VisitUnaryExpression([NotNull] GLangParser.UnaryExpressionContext context) {
			bool hasAtom = context.atom != null;
			bool hasOperator = context.op != null;
			bool hasExpression = context.binaryExpression() != null;

			if ((!hasOperator || !hasExpression) && !hasAtom)
				return Error(Diagnostic.ReportInvalidUnaryExpression(context.Start.Line, context.Start.Column, context.GetText()));

			if (hasAtom)
				return Visit(context.atom);

			var visit = Visit(context.binaryExpression());

			if (!(visit is ExpressionSyntax))
				return Error(Diagnostic.ReportInvalidUnaryExpression(context.Start.Line, context.Start.Column, context.GetText()));

			return new UnaryExpressionSyntax(Token(context.op), (ExpressionSyntax)visit);
		}

		public override SyntaxNode VisitBinaryExpression([NotNull] GLangParser.BinaryExpressionContext context) {
			bool hasAtom = context.atom != null;
			bool hasLeft = context.left != null;
			bool hasOperator = context.op != null;
			bool hasRight = context.right != null;

			if ((!hasLeft || !hasOperator || !hasRight) && !hasAtom)
				return Error(Diagnostic.ReportInvalidBinaryExpression(context.Start.Line, context.Start.Column, context.GetText()));

			if (hasAtom)
				return Visit(context.atom);

			var visitLeft = Visit(context.left);
			var visitRight = Visit(context.right);

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

			var expressionCtx = context.binaryExpression();
			var commasCtx = context.COMMA();

			if (funcIdentifierCtx == null || leftParenCtx == null || rightParenCtx == null)
				return Error(Diagnostic.ReportInvalidFunctionCall(context.Start.Line, context.Start.Column, context.GetText()));

			if (commasCtx != null)
				writer.WriteLine($"expression count {expressionCtx.Length}");

			if (expressionCtx != null)
				writer.WriteLine($"expression count {commasCtx.Length}");

			if (expressionCtx != null && commasCtx != null && expressionCtx.Length > 0 && commasCtx.Length > 0) {
				if (expressionCtx.Length < 2 || commasCtx.Length != expressionCtx.Length - 1)
					return Error(Diagnostic.ReportInvalidParameters(context.Start.Line, context.Start.Column, $"{context.GetText()} {expressionCtx.Length} {commasCtx.Length}"));
			}

			if (expressionCtx != null && commasCtx == null && expressionCtx.Length > 0) {
				if (expressionCtx.Length != 1)
					return Error(Diagnostic.ReportInvalidParameters(expressionCtx[0].Start.Line, expressionCtx[0].Start.Column, $"{context.GetText()} {expressionCtx.Length} {commasCtx.Length}"));
			}

			var seperatedList = new List<SyntaxNode>();
			if (expressionCtx != null && expressionCtx.Length > 0) {
				for (int i = 0; i < expressionCtx.Length - 1; i++) {
					var exprVisit = Visit(expressionCtx[i]);
					var commaToken = Token(commasCtx[i].Symbol);

					if (!(exprVisit is ExpressionSyntax))
						return Error(Diagnostic.ReportInvalidParameter(expressionCtx[i].Start.Line, expressionCtx[i].Start.Column, expressionCtx[i].GetText()));

					seperatedList.Add(exprVisit);
					seperatedList.Add(commaToken);
				}

				var lastCtx = expressionCtx[expressionCtx.Length - 1];
				var lastExprVisit = Visit(lastCtx);

				if (!(lastExprVisit is ExpressionSyntax))
					return Error(Diagnostic.ReportInvalidParameter(lastCtx.Start.Line, lastCtx.Start.Column, lastCtx.GetText()));

				seperatedList.Add(lastExprVisit);
			}

			return new FunctionCallSyntax(Token(funcIdentifierCtx), Token(leftParenCtx.Symbol), new SeperatedSyntaxList<ExpressionSyntax>(seperatedList), Token(rightParenCtx.Symbol));
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

			var paramsCtx = context.definedIdentifier();
			var commasCtx = context.COMMA();

			if (leftParenCtx == null || rightParenCtx == null || !leftParenCtx.GetText().Equals("(") || !rightParenCtx.GetText().Equals(")"))
				return Error(Diagnostic.ReportInvalidParameterDefinition(context.Start.Line, context.Start.Column, context.GetText()));

			if (paramsCtx != null && paramsCtx.Length > 0 && (commasCtx == null || commasCtx.Length <= 0) && paramsCtx.Length != 1)
				return Error(Diagnostic.ReportInvalidParameterDefinition(context.Start.Line, context.Start.Column, context.GetText()));

			if (paramsCtx != null && commasCtx != null && paramsCtx.Length > 0 && commasCtx.Length >0 && (paramsCtx.Length < 2 || commasCtx.Length != paramsCtx.Length - 1))
				return Error(Diagnostic.ReportInvalidParameterDefinition(context.Start.Line, context.Start.Column, context.GetText()));

			var paramsList = new List<SyntaxNode>();

			if (paramsCtx != null && paramsCtx.Length > 0) {
				for (int i = 0; i < paramsCtx.Length - 1; i++) {
					var current = paramsCtx[i];
					var currentVisit = Visit(current);

					if (!(currentVisit is TypedIdentifierSyntax))
						return Error(Diagnostic.ReportInvalidParameterDefinition(current.Start.Line, current.Start.Column, current.GetText()));

					paramsList.Add((TypedIdentifierSyntax)currentVisit);
					paramsList.Add(Token(commasCtx[i].Symbol));
				}

				var lastParam = paramsCtx[paramsCtx.Length - 1];
				var lastVisit = Visit(lastParam);

				if (!(lastVisit is TypedIdentifierSyntax))
					return Error(Diagnostic.ReportInvalidParameterDefinition(lastParam.Start.Line, lastParam.Start.Column, lastParam.GetText()));

				paramsList.Add((TypedIdentifierSyntax)lastVisit);
			}

			return new ParameterDefinitionSyntax(Token(leftParenCtx.Symbol), new SeperatedSyntaxList<TypedIdentifierSyntax>(paramsList), Token(rightParenCtx.Symbol));
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
			return new FunctionDeclarationSyntax(keywToken, identToken, parameters, typeDef, body);
		}
	}
}
