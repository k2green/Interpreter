using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using InterpreterLib.Syntax.Tree;
using InterpreterLib.Syntax.Tree.Expressions;
using InterpreterLib.Syntax.Tree.Statements;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InterpreterLib.Syntax {
	internal class ASTProducer : GLangBaseVisitor<SyntaxNode> {

		private DiagnosticContainer diagnostics;

		public ASTProducer() {
			diagnostics = new DiagnosticContainer();
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
			var identifierCtx = context.IDENTIFIER();
			var typeDefCtx = context.typeDefinition();
			var assignmentCtx = context.assignmentExpression();

			bool hasIdentifier = identifierCtx != null;
			bool hasTypedef = typeDefCtx != null;
			bool hasDirectDecl = hasIdentifier && hasTypedef;
			bool isAssignmentDecl = assignmentCtx != null;

			if (keywordCtx == null || !(keywordCtx.Symbol.Text.Equals("var") || keywordCtx.Symbol.Text.Equals("var")))
				return Error(Diagnostic.ReportInvalidDeclarationKeyword(context.Start.Line, context.Start.Column, context.GetText()));

			if (hasIdentifier ^ hasTypedef)
				return Error(Diagnostic.ReportMalformedDeclaration(keywordCtx.Symbol.Line, keywordCtx.Symbol.Column, context.GetText()));

			if (!OnlyOne(hasDirectDecl, isAssignmentDecl))
				return Error(Diagnostic.ReportMalformedDeclaration(keywordCtx.Symbol.Line, keywordCtx.Symbol.Column, context.GetText()));

			if (hasDirectDecl) {
				var defVisit = Visit(context.typeDefinition());

				if (!(defVisit is TypeDefinitionSyntax))
					return Error(Diagnostic.ReportMalformedDeclaration(keywordCtx.Symbol.Line, keywordCtx.Symbol.Column, context.GetText()));

				return new VariableDeclarationSyntax(Token(keywordCtx.Symbol), Token(identifierCtx.Symbol), (TypeDefinitionSyntax)defVisit, null, null);
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

			if (context.assignmentExpression() != null)
				return Visit(context.assignmentExpression());

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
				return Error(Diagnostic.ReportInvalisWhileStatement(context.Start.Line, context.Start.Column, context.GetText()));

			if (!whileKeywCtx.GetText().Equals("while") || !lParenCtx.GetText().Equals("(") || !rParenCtx.GetText().Equals(")"))
				return Error(Diagnostic.ReportInvalidIfStatement(context.Start.Line, context.Start.Column, context.GetText()));

			var condVisit = Visit(conditionCtx);
			var bodyVisit = Visit(bodyCtx);

			if (!(condVisit is ExpressionSyntax && bodyVisit is StatementSyntax))
				return Error(Diagnostic.ReportInvalisWhileStatement(context.Start.Line, context.Start.Column, context.GetText()));

			return new WhileLoopSyntax(Token(whileKeywCtx.Symbol), Token(lParenCtx.Symbol), (ExpressionSyntax)condVisit, Token(rParenCtx.Symbol), (StatementSyntax)bodyVisit);
		}
	}
}
