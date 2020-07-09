using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using InterpreterLib.Syntax.Tree;
using InterpreterLib.Syntax.Tree.Expressions;
using InterpreterLib.Syntax.Tree.Global;
using InterpreterLib.Syntax.Tree.Statements;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
			bool hasFunctionCall = context.functionCall() != null;

			// Returns an error if there isn'n exactly one token
			if (!(OnlyOne(hasInt, hasIdentifier, hasBool, hasString, hasDouble, hasFunctionCall))) {
				var span = new TextSpan(context.Start.StartIndex, context.Stop.StopIndex);
				diagnostics.AddDiagnostic(Diagnostic.ReportInvalidLiteral(context.Start.Line, context.Start.Column, span));

				return null;
			}

			// Return a literal of variable Syntax depending on which token exists
			if (hasDouble)
				return new LiteralSyntax(new TokenSyntax(context.DOUBLE().Symbol));

			if (hasInt)
				return new LiteralSyntax(new TokenSyntax(context.INTEGER().Symbol));

			if (hasFunctionCall)
				return Visit(context.functionCall());

			if (hasBool)
				return new LiteralSyntax(new TokenSyntax(context.BOOLEAN().Symbol));

			if (hasIdentifier)
				return new VariableSyntax(new TokenSyntax(context.IDENTIFIER().Symbol));

			if (hasString)
				return new LiteralSyntax(new TokenSyntax(context.STRING().Symbol));

			// As a last resort, returns an error.
			return null;
		}

		public override SyntaxNode VisitUnaryExpression([NotNull] GLangParser.UnaryExpressionContext context) {

			bool hasAtom = context.atom != null;
			bool hasUnaryOperation = context.op != null && context.unaryExpression() != null;
			bool hasExpression = context.L_PARENTHESIS() != null && context.binaryExpression() != null && context.R_PARENTHESIS() != null;

			if (!OnlyOne(hasAtom, hasUnaryOperation, hasExpression)) {
				diagnostics.AddDiagnostic(Diagnostic.ReportInvalidUnaryExpression(context.Start.Line, context.Start.Column, new TextSpan(context.Start.StartIndex, context.Stop.StopIndex)));
				return null;
			}

			// Visit the atom if it exists
			if (hasAtom)
				return Visit(context.atom);

			if (hasExpression)
				return Visit(context.binaryExpression());


			// Visit the binary expression
			var unaryCtx = context.unaryExpression();
			var visit = Visit(unaryCtx);

			if (visit == null)
				return null;

			// Ensures the visit is an expression
			if (!(visit is ExpressionSyntax)) {
				var prev = new TextSpan(context.op.StartIndex, context.op.StopIndex);
				return Error(Diagnostic.ReportInvalidUnaryExpression(context.Start.Line, context.Start.Column, prev, visit.Span));
			}

			return new UnaryExpressionSyntax(Token(context.op), (ExpressionSyntax)visit);
		}

		public override SyntaxNode VisitBinaryExpression([NotNull] GLangParser.BinaryExpressionContext context) {
			bool hasAtom = context.atom != null && !string.IsNullOrEmpty(context.atom.GetText());
			bool hasLeft = context.left != null && !string.IsNullOrEmpty(context.left.GetText());
			bool hasOperator = context.op != null && !string.IsNullOrEmpty(context.op.Text);
			bool hasRight = context.right != null && !string.IsNullOrEmpty(context.right.GetText());

			if (hasAtom && (hasOperator || hasLeft || hasRight)) {
				diagnostics.AddDiagnostic(Diagnostic.ReportInvalidBinaryExpression(context.atom.Start.Line, context.atom.Start.Column, new TextSpan(context.atom.Start.StartIndex, context.atom.Stop.StopIndex)));
				return null;
			}

			// Ensures that either the context has both a left and right expression with an operator, or that it has an atom.
			if (hasOperator && (!hasLeft || !hasRight)) {
				diagnostics.AddDiagnostic(Diagnostic.ReportInvalidBinaryExpression(context.Start.Line, context.Start.Column, new TextSpan(context.Start.StartIndex, context.Stop.StopIndex)));
				return null;
			}

			// Visit the atom if it exists
			if (hasAtom)
				return Visit(context.unaryExpression());

			// Visit the sub-expression
			var visitLeft = Visit(context.left);
			var visitRight = Visit(context.right);

			if (visitLeft == null || visitRight == null)
				return null;

			// Ensures the visis are expressions
			if (!(visitLeft is ExpressionSyntax left)) {
				var next = new TextSpan(context.op.StartIndex, context.right.Stop.StopIndex);

				diagnostics.AddDiagnostic(Diagnostic.ReportInvalidBinaryExpression(context.left.Start.Line, context.left.Start.Column, null, visitLeft.Span, next));
				return null;
			}

			if (!(visitRight is ExpressionSyntax right)) {
				var prev = new TextSpan(context.left.Start.StartIndex, context.op.StopIndex);

				diagnostics.AddDiagnostic(Diagnostic.ReportInvalidBinaryExpression(context.right.Start.Line, context.right.Start.Column, prev, visitRight.Span, null));
				return null;
			}

			return new BinaryExpressionSyntax(left, Token(context.op), right);
		}

		public override SyntaxNode VisitTypeDefinition([NotNull] GLangParser.TypeDefinitionContext context) {
			var delimeterCtx = context.TYPE_DELIMETER();
			var nameCtx = context.TYPE_NAME();

			if (delimeterCtx == null || !delimeterCtx.GetText().Equals(":")) {
				diagnostics.AddDiagnostic(Diagnostic.ReportMissingToken(context.Start.Line, context.Start.Column, new TextSpan(context.Start.StartIndex, context.Stop.StopIndex), "type delimeter"));
				return null;
			}

			if (delimeterCtx == null) {
				diagnostics.AddDiagnostic(Diagnostic.ReportMissingToken(context.Start.Line, context.Start.Column, new TextSpan(context.Start.StartIndex, context.Stop.StopIndex), "type name"));
				return null;
			}

			return new TypeDefinitionSyntax(new TokenSyntax(delimeterCtx.Symbol), new TokenSyntax(nameCtx.Symbol));
		}

		public override SyntaxNode VisitAssignmentExpression([NotNull] GLangParser.AssignmentExpressionContext context) {
			var identifierCtx = context.IDENTIFIER();
			var typeDefCtx = context.typeDefinition();
			var operatorCtx = context.ASSIGNMENT_OPERATOR();
			var expressionCtx = context.binaryExpression();

			if (identifierCtx == null) {
				diagnostics.AddDiagnostic(Diagnostic.ReportMissingToken(context.Start.Line, context.Start.Column, new TextSpan(context.Start.StartIndex, context.Stop.StopIndex), "identifier"));
				return null;
			}

			if (operatorCtx == null) {
				diagnostics.AddDiagnostic(Diagnostic.ReportMissingToken(context.Start.Line, context.Start.Column, new TextSpan(context.Start.StartIndex, context.Stop.StopIndex), "assignment operator"));
				return null;
			}

			if (expressionCtx == null) {
				diagnostics.AddDiagnostic(Diagnostic.ReportMissingToken(context.Start.Line, context.Start.Column, new TextSpan(context.Start.StartIndex, context.Stop.StopIndex), "expression"));
				return null;
			}

			TypeDefinitionSyntax typeDef = null;
			if (typeDefCtx != null) {
				var visit = Visit(typeDefCtx);

				if (visit == null)
					return null;

				if (!(visit is TypeDefinitionSyntax)) {
					var prev = new TextSpan(identifierCtx.Symbol.StartIndex, identifierCtx.Symbol.StopIndex);
					var next = new TextSpan(operatorCtx.Symbol.StartIndex, operatorCtx.Symbol.StopIndex);

					diagnostics.AddDiagnostic(Diagnostic.ReportInvalidTypeDefinition(typeDefCtx.Start.Line, typeDefCtx.Start.Column, prev, visit.Span, next));
					return null;
				}

				typeDef = (TypeDefinitionSyntax)visit;
			}

			var visitExpr = Visit(expressionCtx);

			if (visitExpr == null)
				return null;

			if (!(visitExpr is ExpressionSyntax)) {
				var prev = new TextSpan(operatorCtx.Symbol.StartIndex, operatorCtx.Symbol.StopIndex);

				diagnostics.AddDiagnostic(Diagnostic.ReportInvalidAssignmentOperand(expressionCtx.Start.Line, expressionCtx.Start.Column, prev, visitExpr.Span));
				return null;
			}

			return new AssignmentExpressionSyntax(Token(identifierCtx.Symbol), typeDef, Token(operatorCtx.Symbol), (ExpressionSyntax)visitExpr);
		}

		public override SyntaxNode VisitVariableDeclarationStatement([NotNull] GLangParser.VariableDeclarationStatementContext context) {
			var keywordCtx = context.DECL_VARIABLE();
			var identifierCtx = context.definedIdentifier();
			var assignmentCtx = context.assignmentExpression();

			bool hasDirectDecl = identifierCtx != null;
			bool isAssignmentDecl = assignmentCtx != null;

			if (!OnlyOne(hasDirectDecl, isAssignmentDecl)) {
				diagnostics.AddDiagnostic(Diagnostic.ReportMalformedDeclaration(keywordCtx.Symbol.Line, keywordCtx.Symbol.Column, new TextSpan(context.Start.StartIndex, context.Stop.StopIndex)));
				return null;
			}

			if (keywordCtx == null || !(keywordCtx.Symbol.Text.Equals("var") || keywordCtx.Symbol.Text.Equals("var"))) {
				var span = new TextSpan(context.Start.StartIndex, hasDirectDecl ? identifierCtx.Start.StartIndex : assignmentCtx.Start.StartIndex);
				diagnostics.AddDiagnostic(Diagnostic.ReportInvalidDeclarationKeyword(context.Start.Line, context.Start.Column, span));
				return null;
			}

			if (hasDirectDecl) {
				var defVisit = Visit(identifierCtx);

				if (defVisit == null)
					return null;

				if (!(defVisit is TypedIdentifierSyntax)) {
					diagnostics.AddDiagnostic(Diagnostic.ReportMalformedDeclaration(identifierCtx.Start.Line, identifierCtx.Start.Column, defVisit.Span));
					return null;
				}

				var def = (TypedIdentifierSyntax)defVisit;
				return new VariableDeclarationSyntax(Token(keywordCtx.Symbol), def.Identifier, def.Definition, null, null);
			} else {
				var assignVisit = Visit(assignmentCtx);

				if (assignVisit == null)
					return null;

				if (!(assignVisit is AssignmentExpressionSyntax assignExpr)) {
					diagnostics.AddDiagnostic(Diagnostic.ReportMalformedDeclaration(keywordCtx.Symbol.Line, keywordCtx.Symbol.Column, assignVisit.Span));
					return null;
				}

				return new VariableDeclarationSyntax(Token(keywordCtx.Symbol), assignExpr.IdentifierToken, assignExpr.Definition, assignExpr.OperatorToken, assignExpr.Expression);
			}
		}

		public override SyntaxNode VisitExpressionStatement([NotNull] GLangParser.ExpressionStatementContext context) {
			var assignmentCtx = context.assignmentExpression();
			var expressionCtx = context.binaryExpression();

			if (!OnlyOne(assignmentCtx != null, expressionCtx != null)) {
				diagnostics.AddDiagnostic(Diagnostic.ReportInvalidExpressionStatement(context.Start.Line, context.Start.Column, new TextSpan(context.Start.StartIndex, context.Stop.StopIndex)));
				return null;
			}
			SyntaxNode visit;

			if (assignmentCtx != null)
				visit = Visit(assignmentCtx);
			else
				visit = Visit(expressionCtx);

			if (visit == null)
				return null;

			if (!(visit is ExpressionSyntax)) {
				diagnostics.AddDiagnostic(Diagnostic.ReportInvalidExpressionStatement(context.Start.Line, context.Start.Column, visit.Span));
				return null;
			}

			return new ExpressionStatementSyntax((ExpressionSyntax)visit);
		}

		public override SyntaxNode VisitDeclerationOrAssign([NotNull] GLangParser.DeclerationOrAssignContext context) {
			if (context.variableDeclarationStatement() != null)
				return Visit(context.variableDeclarationStatement());

			if (context.assignmentExpression() != null) {
				var visit = Visit(context.assignmentExpression());

				if (visit == null)
					return null;

				if (!(visit is ExpressionSyntax)) {
					var span = new TextSpan(context.Start.StartIndex, context.Stop.StopIndex);
					diagnostics.AddDiagnostic(Diagnostic.ReportInvalidInternalForAssignment(context.Start.Line, context.Start.Column, span));
					return null;
				}

				return new ExpressionStatementSyntax((ExpressionSyntax)visit);
			}

			return null;
		}

		public override SyntaxNode VisitIfStatement([NotNull] GLangParser.IfStatementContext context) {
			var ifKeywCtx = context.IF();
			var lParenCtx = context.L_PARENTHESIS();
			var conditionCtx = context.binaryExpression();
			var rParenCtx = context.R_PARENTHESIS();
			var trueCtx = context.trueBranch;
			var elseCtx = context.ELSE();
			var falseCtx = context.falseBranch;

			if (ifKeywCtx == null || conditionCtx == null || lParenCtx == null || rParenCtx == null || trueCtx == null) {
				var diagnostic = Diagnostic.ReportInvalidIfStatement(context.Start.Line, context.Start.Column, new TextSpan(context.Start.StartIndex, context.Stop.StopIndex));
				diagnostics.AddDiagnostic(diagnostic);
				return null;
			}

			if (!ifKeywCtx.GetText().Equals("if") || !lParenCtx.GetText().Equals("(") || !rParenCtx.GetText().Equals(")")) {
				var span = new TextSpan(ifKeywCtx.Symbol.StartIndex, ifKeywCtx.Symbol.StopIndex);
				var next = new TextSpan(lParenCtx.Symbol.StartIndex, rParenCtx.Symbol.StopIndex);

				var diagnostic = Diagnostic.ReportInvalidIfStatement(ifKeywCtx.Symbol.Line, ifKeywCtx.Symbol.Column, null, span, next);
				diagnostics.AddDiagnostic(diagnostic);
				return null;
			}

			if (elseCtx == null ^ falseCtx == null) {
				var prev = new TextSpan(trueCtx.Start.StartIndex, trueCtx.Stop.StopIndex);
				TextSpan Span;

				if (elseCtx != null)
					Span = new TextSpan(elseCtx.Symbol.StartIndex, elseCtx.Symbol.StopIndex);
				else
					Span = new TextSpan(falseCtx.Start.StartIndex, falseCtx.Stop.StopIndex);

				diagnostics.AddDiagnostic(Diagnostic.ReportInvalidIfStatement(context.Start.Line, context.Start.Column, prev, Span, null));
				return null;
			}

			if (elseCtx != null && !elseCtx.GetText().Equals("else")) {
				var prev = new TextSpan(trueCtx.Start.StartIndex, trueCtx.Stop.StopIndex);
				var span = new TextSpan(elseCtx.Symbol.StartIndex, elseCtx.Symbol.StopIndex);
				var next = new TextSpan(falseCtx.Start.StartIndex, falseCtx.Stop.StopIndex);

				diagnostics.AddDiagnostic(Diagnostic.ReportInvalidIfStatement(elseCtx.Symbol.Line, elseCtx.Symbol.Column, prev, span, next));
				return null;
			}

			var condVisit = Visit(conditionCtx);
			var trueVisit = Visit(trueCtx);

			if (condVisit == null || trueVisit == null)
				return null;

			if (!(condVisit is ExpressionSyntax condition)) {
				var prev = new TextSpan(lParenCtx.Symbol.StartIndex, lParenCtx.Symbol.StopIndex);
				var next = new TextSpan(rParenCtx.Symbol.StartIndex, rParenCtx.Symbol.StopIndex);

				diagnostics.AddDiagnostic(Diagnostic.ReportInvalidIfStatement(elseCtx.Symbol.Line, elseCtx.Symbol.Column, prev, condVisit.Span, next));
				return null;
			}

			if (!(trueVisit is StatementSyntax trueStat)) {
				var prev = new TextSpan(rParenCtx.Symbol.StartIndex, rParenCtx.Symbol.StopIndex);
				TextSpan? next = null;

				if (elseCtx != null)
					next = new TextSpan(elseCtx.Symbol.StartIndex, elseCtx.Symbol.StopIndex);

				diagnostics.AddDiagnostic(Diagnostic.ReportInvalidIfStatement(context.Start.Line, context.Start.Column, prev, trueVisit.Span, next));
				return null;
			}

			TokenSyntax elseToken = null;
			StatementSyntax falseStatement = null;

			if (elseCtx != null) {
				elseToken = Token(elseCtx.Symbol);
				var falseVisit = Visit(falseCtx);

				if (falseVisit == null)
					return null;

				if (!(falseVisit is StatementSyntax)) {
					diagnostics.AddDiagnostic(Diagnostic.ReportInvalidIfStatement(context.Start.Line, context.Start.Column, elseToken.Span, falseVisit.Span, null));
					return null;
				}

				falseStatement = (StatementSyntax)falseVisit;
			}

			return new IfStatementSyntax(Token(ifKeywCtx.Symbol), Token(lParenCtx.Symbol), condition, Token(rParenCtx.Symbol), trueStat, elseToken, falseStatement);
		}

		public override SyntaxNode VisitWhileStatement([NotNull] GLangParser.WhileStatementContext context) {
			var whileKeywCtx = context.WHILE();
			var lParenCtx = context.L_PARENTHESIS();
			var conditionCtx = context.binaryExpression();
			var rParenCtx = context.R_PARENTHESIS();
			var bodyCtx = context.statement();

			if (whileKeywCtx == null || conditionCtx == null || lParenCtx == null || rParenCtx == null) {
				var span = new TextSpan(context.Start.StartIndex, context.Stop.StopIndex);
				diagnostics.AddDiagnostic(Diagnostic.ReportInvalidWhileStatement(context.Start.Line, context.Start.Column, span));
				return null;
			}

			if (!whileKeywCtx.GetText().Equals("while") || !lParenCtx.GetText().Equals("(") || !rParenCtx.GetText().Equals(")")) {
				var span = new TextSpan(whileKeywCtx.Symbol.StartIndex, whileKeywCtx.Symbol.StopIndex);
				diagnostics.AddDiagnostic(Diagnostic.ReportInvalidWhileStatement(context.Start.Line, context.Start.Column, span));
				return null;
			}

			var condVisit = Visit(conditionCtx);
			var bodyVisit = Visit(bodyCtx);

			if (condVisit == null || bodyVisit == null)
				return null;

			if (!(condVisit is ExpressionSyntax condition)) {
				var prev = new TextSpan(lParenCtx.Symbol.StartIndex, lParenCtx.Symbol.StopIndex);
				var next = new TextSpan(rParenCtx.Symbol.StartIndex, rParenCtx.Symbol.StopIndex);

				diagnostics.AddDiagnostic(Diagnostic.ReportInvalidWhileStatement(context.Start.Line, context.Start.Column, prev, condVisit.Span, next));
				return null;
			}

			if (!(bodyVisit is StatementSyntax statement)) {
				var prev = new TextSpan(rParenCtx.Symbol.StartIndex, rParenCtx.Symbol.StopIndex);

				diagnostics.AddDiagnostic(Diagnostic.ReportInvalidWhileStatement(context.Start.Line, context.Start.Column, prev, bodyVisit.Span, null));
				return null;
			}

			return new WhileLoopSyntax(Token(whileKeywCtx.Symbol), Token(lParenCtx.Symbol), condition, Token(rParenCtx.Symbol), statement);
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
				|| conditionCtx == null || stepCtx == null || rParenCtx == null || bodyCtx == null) {

				var span = new TextSpan(context.Start.StartIndex, context.Stop.StopIndex);
				diagnostics.AddDiagnostic(Diagnostic.ReportInvalidForStatement(context.Start.Line, context.Start.Column, span));
				return null;
			}

			if (!forKeywCtx.GetText().Equals("for") || !lParenCtx.GetText().Equals("(") || !rParenCtx.GetText().Equals(")")) {
				var span = new TextSpan(forKeywCtx.Symbol.StartIndex, forKeywCtx.Symbol.StopIndex);
				diagnostics.AddDiagnostic(Diagnostic.ReportInvalidForStatement(context.Start.Line, context.Start.Column, span));
				return null;
			}

			var assignVisit = Visit(context.declerationOrAssign());
			var condVisit = Visit(context.binaryExpression());
			var stepVisit = Visit(context.assignmentExpression());
			var bodyVisit = Visit(context.statement());

			if (assignVisit == null || condVisit == null || stepVisit == null || bodyVisit == null)
				return null;

			if (!(assignVisit is StatementSyntax assignment)) {
				var prev = new TextSpan(lParenCtx.Symbol.StartIndex, lParenCtx.Symbol.StopIndex);

				diagnostics.AddDiagnostic(Diagnostic.ReportInvalidForStatement(context.Start.Line, context.Start.Column, prev, assignVisit.Span, condVisit.Span));
				return null;
			}

			if (!(condVisit is ExpressionSyntax condition)) {
				diagnostics.AddDiagnostic(Diagnostic.ReportInvalidForStatement(context.Start.Line, context.Start.Column, assignVisit.Span, condVisit.Span, stepVisit.Span));
				return null;
			}

			if (!(stepVisit is ExpressionSyntax step)) {
				var next = new TextSpan(rParenCtx.Symbol.StartIndex, rParenCtx.Symbol.StopIndex);

				diagnostics.AddDiagnostic(Diagnostic.ReportInvalidForStatement(context.Start.Line, context.Start.Column, condVisit.Span, stepVisit.Span, next));
				return null;
			}

			if (!(bodyVisit is StatementSyntax body)) {
				var prev = new TextSpan(rParenCtx.Symbol.StartIndex, rParenCtx.Symbol.StopIndex);

				diagnostics.AddDiagnostic(Diagnostic.ReportInvalidForStatement(context.Start.Line, context.Start.Column, prev, bodyVisit.Span, null));
				return null;
			}

			return new ForLoopSyntax(
				Token(forKeywCtx.Symbol),
				Token(lParenCtx.Symbol),
				assignment,
				Token(commasCtx[0].Symbol),
				condition, Token(commasCtx[1].Symbol),
				(ExpressionSyntax)stepVisit,
				Token(rParenCtx.Symbol),
				(StatementSyntax)bodyVisit);
		}

		public override SyntaxNode VisitBlock([NotNull] GLangParser.BlockContext context) {
			var leftBraceCtx = context.L_BRACE();
			var statementsCtx = context.statement();
			var rightBraceCtx = context.R_BRACE();

			bool hasLeftBrace = leftBraceCtx != null && leftBraceCtx.Symbol.Text.Equals("{");
			bool hasRightBrace = rightBraceCtx != null && rightBraceCtx.Symbol.Text.Equals("}");

			if (!hasLeftBrace || !hasRightBrace || statementsCtx == null) {
				var span = new TextSpan(context.Start.StartIndex, context.Stop.StopIndex);
				diagnostics.AddDiagnostic(Diagnostic.ReportInvalidBlock(context.Start.Line, context.Start.Column, span));
				return null;
			}

			var builder = ImmutableArray.CreateBuilder<StatementSyntax>();

			foreach (var ctx in statementsCtx) {
				var visit = Visit(ctx);

				if (visit == null)
					return null;

				if (visit is StatementSyntax statement) {
					builder.Add(statement);
				} else {

					return Error(Diagnostic.ReportInvalidStatement(visit.Location.Line, visit.Location.Column, visit.Span));
				}
			}

			return new BlockSyntax(Token(leftBraceCtx.Symbol), builder.ToImmutable(), Token(rightBraceCtx.Symbol));
		}

		public override SyntaxNode VisitFunctionCall([NotNull] GLangParser.FunctionCallContext context) {
			var funcIdentifierCtx = context.funcName;
			var leftParenCtx = context.L_PARENTHESIS();
			var rightParenCtx = context.R_PARENTHESIS();
			var span = new TextSpan(context.Start.StartIndex, context.Stop.StopIndex);

			if (funcIdentifierCtx == null || leftParenCtx == null || rightParenCtx == null || !leftParenCtx.GetText().Equals("(") || !rightParenCtx.GetText().Equals(")")) {
				diagnostics.AddDiagnostic(Diagnostic.ReportInvalidFunctionCall(context.Start.Line, context.Start.Column, span));
				return null;
			}

			var firstCtx = context.seperatedExpression();
			var lastCtx = context.last;

			if (firstCtx != null && firstCtx.Length > 0 && lastCtx == null) {
				diagnostics.AddDiagnostic(Diagnostic.ReportInvalidCallParameters(context.Start.Line, context.Start.Column, span));
				return null;
			}

			var builder = ImmutableArray.CreateBuilder<SyntaxNode>();

			if (firstCtx != null && firstCtx.Length > 0 && lastCtx != null) {
				foreach (var ctx in firstCtx) {
					var expressionCtx = ctx.binaryExpression();
					var commaCtx = ctx.COMMA();

					var visit = Visit(expressionCtx);

					if (visit == null)
						return null;

					if (!(visit is ExpressionSyntax expressionSyntax)) {
						diagnostics.AddDiagnostic(Diagnostic.ReportInvalidCallParameter(expressionCtx.Start.Line, expressionCtx.Start.Column, visit.Span));
						return null;
					}

					if (commaCtx == null || !commaCtx.GetText().Equals(",")) {
						var commaSpan = new TextSpan(visit.Span.End, ctx.Stop.StopIndex);
						diagnostics.AddDiagnostic(Diagnostic.ReportMissingComma(expressionCtx.Start.Line, expressionCtx.Start.Column, visit.Span, commaSpan, null));
						return null;
					}

					builder.Add(expressionSyntax);
					builder.Add(Token(commaCtx.Symbol));
				}
			}

			if (lastCtx != null) {
				var lastVisit = Visit(lastCtx);

				if (lastVisit == null)
					return null;

				if (!(lastVisit is ExpressionSyntax lastExpressionSyntax)) {
					var commaSpan = new TextSpan(lastCtx.Start.StartIndex, lastCtx.Stop.StopIndex);
					diagnostics.AddDiagnostic(Diagnostic.ReportInvalidCallParameter(lastVisit.Location.Line, lastVisit.Location.Column, lastVisit.Span));
					return null;
				}

				builder.Add(lastExpressionSyntax);
			}

			var seperatedList = new SeperatedSyntaxList<ExpressionSyntax>(builder.ToImmutable());
			return new FunctionCallSyntax(Token(funcIdentifierCtx), Token(leftParenCtx.Symbol), seperatedList, Token(rightParenCtx.Symbol));
		}

		public override SyntaxNode VisitErrorNode(IErrorNode node) {
			diagnostics.AddDiagnostic(Diagnostic.ReportSyntaxError(node.Symbol.Line, node.Symbol.Column, new TextSpan(node.Symbol.StartIndex, node.Symbol.StopIndex)));
			return null;
		}

		public override SyntaxNode VisitBaseStatement([NotNull] GLangParser.BaseStatementContext context) {
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

			return null;
		}

		public override SyntaxNode VisitCompilationUnit([NotNull] GLangParser.CompilationUnitContext context) {
			var statementsCtx = context.globalStatement();
			var statements = ImmutableArray.CreateBuilder<GlobalSyntax>();

			if (statementsCtx == null || statementsCtx.Length < 1) {
				var span = new TextSpan(context.Start.StartIndex, context.Stop.StopIndex);
				diagnostics.AddDiagnostic(Diagnostic.ReportInvalidCompilationUnit(context.Start.Line, context.Start.Column, span));
				return null;
			}

			foreach (var statementCtx in statementsCtx) {
				var visitStatement = Visit(statementCtx);

				if (visitStatement == null)
					return null;

				if (!(visitStatement is GlobalSyntax globalSyntax)) {
					diagnostics.AddDiagnostic(Diagnostic.ReportInvalidGlobalStatement(visitStatement.Location.Line, visitStatement.Location.Column, visitStatement.Span));
					return null;
				}

				statements.Add(globalSyntax);
			}

			return new CompilationUnitSyntax(statements.ToImmutable());
		}

		public override SyntaxNode VisitStatement([NotNull] GLangParser.StatementContext context) {
			var baseCtx = context.baseStatement();
			var breakCtx = context.BREAK();
			var continueCtx = context.CONTINUE();

			bool hasBreak = breakCtx != null && breakCtx.Symbol.Text.Equals("break");
			bool hasContinue = continueCtx != null && continueCtx.Symbol.Text.Equals("continue");

			if (!OnlyOne(baseCtx != null, hasBreak, hasContinue)) {
				var span = new TextSpan(context.Start.StartIndex, context.Stop.StopIndex);
				diagnostics.AddDiagnostic(Diagnostic.ReportInvalidStatement(context.Start.Line, context.Start.Column, span));
				return null;
			}

			if (baseCtx != null)
				return Visit(baseCtx);

			if (hasBreak)
				return new BreakSyntax(Token(breakCtx.Symbol));

			if (hasContinue)
				return new ContinueSyntax(Token(continueCtx.Symbol));

			return null;
		}

		public override SyntaxNode VisitGlobalStatement([NotNull] GLangParser.GlobalStatementContext context) {
			if (context.functionDefinition() != null)
				return Visit(context.functionDefinition());

			if (context.baseStatement() != null) {
				var statementVisit = Visit(context.baseStatement());

				if (statementVisit == null)
					return null;

				if (!(statementVisit is StatementSyntax statement)) {
					diagnostics.AddDiagnostic(Diagnostic.ReportInvalidGlobalStatement(statementVisit.Location.Line, statementVisit.Location.Column, statementVisit.Span));
					return null;
				}

				return new GlobalStatementSyntax(statement);
			}

			return null;
		}

		public override SyntaxNode VisitDefinedIdentifier([NotNull] GLangParser.DefinedIdentifierContext context) {
			var identifierCtx = context.IDENTIFIER();
			var typeDefCtx = context.typeDefinition();

			if (identifierCtx == null || typeDefCtx == null) {
				var span = new TextSpan(context.Start.StartIndex, context.Stop.StopIndex);
				diagnostics.AddDiagnostic(Diagnostic.ReportInvalidTypedIdentifier(context.Start.Line, context.Start.Column, span));
				return null;
			}

			var typeDefVisit = Visit(typeDefCtx);

			if (!(typeDefVisit is TypeDefinitionSyntax typeDef)) {
				var prev = new TextSpan(identifierCtx.Symbol.StartIndex, identifierCtx.Symbol.StopIndex);
				diagnostics.AddDiagnostic(Diagnostic.ReportInvalidTypeDefinition(context.Start.Line, context.Start.Column, prev, typeDefVisit.Span, null));
				return null;
			}

			return new TypedIdentifierSyntax(Token(identifierCtx.Symbol), typeDef);
		}

		public override SyntaxNode VisitParametersDefinition([NotNull] GLangParser.ParametersDefinitionContext context) {
			var leftParenCtx = context.L_PARENTHESIS();
			var rightParenCtx = context.R_PARENTHESIS();

			var firstCtx = context.seperatedDefinedIdentifier();
			var lastCtx = context.last;
			var span = new TextSpan(context.Start.StartIndex, context.Stop.StopIndex);

			if (leftParenCtx == null || rightParenCtx == null || !leftParenCtx.GetText().Equals("(") || !rightParenCtx.GetText().Equals(")")) {
				diagnostics.AddDiagnostic(Diagnostic.ReportInvalidParameterDefinition(context.Start.Line, context.Start.Column, span));
				return null;
			}

			if (firstCtx != null && firstCtx.Length > 0 && lastCtx == null) {
				diagnostics.AddDiagnostic(Diagnostic.ReportInvalidParameterDefinition(context.Start.Line, context.Start.Column, span));
				return null;
			}

			var parameters = ImmutableArray.CreateBuilder<SyntaxNode>();

			if (firstCtx != null && firstCtx.Length > 0) {
				foreach (var ctx in firstCtx) {
					if (ctx.definedIdentifier() == null || ctx.COMMA() == null || !ctx.COMMA().GetText().Equals(",")) {
						diagnostics.AddDiagnostic(Diagnostic.ReportInvalidParameterDefinition(context.Start.Line, context.Start.Column, span));
						return null;
					}

					var visit = Visit(ctx.definedIdentifier());

					if (visit == null)
						return null;

					if (!(visit is TypedIdentifierSyntax identSyntax)) {
						diagnostics.AddDiagnostic(Diagnostic.ReportInvalidTypedIdentifier(visit.Location.Line, visit.Location.Column, visit.Span));
						return null;
					}

					parameters.Add(identSyntax);
					parameters.Add(Token(ctx.COMMA().Symbol));
				}
			}

			if (lastCtx != null) {

				var lastVisit = Visit(lastCtx);

				if (lastVisit == null)
					return null;

				if (!(lastVisit is TypedIdentifierSyntax lastIdentSyntax)) {
					diagnostics.AddDiagnostic(Diagnostic.ReportInvalidTypedIdentifier(lastVisit.Location.Line, lastVisit.Location.Column, lastVisit.Span));
					return null;
				}

				parameters.Add(lastIdentSyntax);
			}

			var seperatedList = new SeperatedSyntaxList<TypedIdentifierSyntax>(parameters.ToImmutable());
			return new ParameterDefinitionSyntax(Token(leftParenCtx.Symbol), seperatedList, Token(rightParenCtx.Symbol));
		}

		public override SyntaxNode VisitFunctionDefinition([NotNull] GLangParser.FunctionDefinitionContext context) {
			var keywCtx = context.FUNCTION();
			var identCtx = context.IDENTIFIER();
			var paramCtx = context.parametersDefinition();
			var typeDefCtx = context.typeDefinition();
			var bodyCtx = context.statement();

			if (keywCtx == null || identCtx == null || paramCtx == null || typeDefCtx == null || bodyCtx == null || !keywCtx.GetText().Equals("function")) {
				var span = new TextSpan(context.Start.StartIndex, context.Stop.StopIndex);
				diagnostics.AddDiagnostic(Diagnostic.ReportInvalidFunctionDef(context.Start.Line, context.Start.Column, span));
				return null;
			}

			var paramVisit = Visit(paramCtx);
			var typeDefVisit = Visit(typeDefCtx);
			var bodyVisit = Visit(bodyCtx);

			if (paramVisit == null || typeDefVisit == null || bodyVisit == null)
				return null;

			if (!(paramVisit is ParameterDefinitionSyntax parameters)) {
				diagnostics.AddDiagnostic(Diagnostic.ReportInvalidFunctionDef(context.Start.Line, context.Start.Column, null, paramVisit.Span, typeDefVisit.Span));
				return null;
			}

			if (!(typeDefVisit is TypeDefinitionSyntax typeDef)) {
				diagnostics.AddDiagnostic(Diagnostic.ReportInvalidFunctionDef(context.Start.Line, context.Start.Column, paramVisit.Span, typeDefVisit.Span, bodyVisit.Span));
				return null;
			}

			if (!(bodyVisit is StatementSyntax body)) {
				diagnostics.AddDiagnostic(Diagnostic.ReportInvalidFunctionDef(context.Start.Line, context.Start.Column, typeDefVisit.Span, bodyVisit.Span, null));
				return null;
			}

			var keywToken = Token(keywCtx.Symbol);
			var identToken = Token(identCtx.Symbol);

			var declSyntax = new FunctionDeclarationSyntax(keywToken, identToken, parameters, typeDef, body);

			functionDeclarations.Add(declSyntax);
			return declSyntax;
		}
	}
}
