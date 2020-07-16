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
		private ExpressionSyntax returnExpression = null;

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
			bool hasString = context.STRING() != null;
			bool hasChar = context.CHAR_LITERAL() != null;
			bool hasByte = context.BYTE() != null;
			bool hasAccessor = context.accessorExpression() != null;

			// Returns an error if there isn'n exactly one token
			if (!(OnlyOne(hasInt, hasBool, hasString, hasDouble, hasChar, hasByte, hasAccessor))) {
				var span = new TextSpan(context.Start.StartIndex, context.Stop.StopIndex);
				diagnostics.AddDiagnostic(Diagnostic.ReportInvalidLiteral(context.Start.Line, context.Start.Column, span));

				return null;
			}

			// Return a literal of variable Syntax depending on which token exists
			if (hasDouble) {
				var token = Token(context.DOUBLE().Symbol);
				return new LiteralSyntax(token, double.Parse(token.ToString()));
			}

			if (hasInt) {
				var token = Token(context.INTEGER().Symbol);
				return new LiteralSyntax(token, int.Parse(token.ToString()));
			}

			if (hasByte) {
				var token = Token(context.BYTE().Symbol);
				var tokenText = token.ToString();
				return new LiteralSyntax(token, byte.Parse(tokenText.Substring(0, tokenText.Length - 1)));
			}

			if (hasBool) {
				var token = Token(context.BOOLEAN().Symbol);
				return new LiteralSyntax(token, bool.Parse(token.ToString()));
			}

			if (hasString) {
				var token = Token(context.STRING().Symbol);
				var tokenText = token.ToString();
				return new LiteralSyntax(token, tokenText.Substring(1, tokenText.Length - 2));
			}

			if (hasChar) {
				var token = Token(context.CHAR_LITERAL().Symbol);
				var tokenText = token.ToString();
				return new LiteralSyntax(token, tokenText[1]);
			}

			if (hasAccessor) {
				return Visit(context.accessorExpression());
			}

			// As a last resort, returns an error.
			return null;
		}

		public override SyntaxNode VisitAccessorExpression([NotNull] GLangParser.AccessorExpressionContext context) {
			var atomCtx = context.accessorAtom();
			var delimeterCtx = context.DOT();
			var restCtx = context.accessorExpression();

			var hasDelimeter = delimeterCtx != null && delimeterCtx.GetText().Equals(".");

			if(atomCtx == null || (hasDelimeter ^ restCtx != null)) {
				return null;
			}

			var atomVisit = Visit(atomCtx);

			if(atomVisit == null || !(atomVisit is AccessorExpressionSyntax atom)) {
				return null;
			}

			var commaToken = delimeterCtx == null ? null : Token(delimeterCtx.Symbol);
			AccessorSyntax restAccessor = null;

			if(restCtx != null) {
				var restVisit = Visit(restCtx);

				if(restVisit == null || !(restVisit is AccessorSyntax rAcc)) {
					return null;
				}

				restAccessor = rAcc;
			}

			return new AccessorSyntax(atom, commaToken, restAccessor);
		}

		public override SyntaxNode VisitAccessorAtom([NotNull] GLangParser.AccessorAtomContext context) {
			var identifierCtx = context.IDENTIFIER();
			var callCtx = context.functionCall();
			var indexerCtx = context.indexedIdentifier();

			if (!(OnlyOne(identifierCtx != null, callCtx != null, indexerCtx != null))) {
				var span = new TextSpan(context.Start.StartIndex, context.Stop.StopIndex);
				diagnostics.AddDiagnostic(Diagnostic.ReportInvalidAccessor(context.Start.Line, context.Start.Column, span));
				return null;
			}

			if (identifierCtx != null)
				return new VariableSyntax(Token(identifierCtx.Symbol));

			if (callCtx != null)
				return Visit(callCtx);

			if (indexerCtx != null)
				return Visit(indexerCtx);

			return null;
		}

		public override SyntaxNode VisitIndexedIdentifier([NotNull] GLangParser.IndexedIdentifierContext context) {
			var identifierCtx = context.IDENTIFIER();
			var lBracket = context.L_BRACKET();
			var expressionCtx = context.binaryExpression();
			var rBracket = context.R_BRACKET();

			bool hasLBracket = lBracket != null && lBracket.GetText().Equals("[");
			bool hasRBracket = rBracket != null && rBracket.GetText().Equals("]");

			if (identifierCtx == null || !hasLBracket || expressionCtx == null || !hasRBracket) {
				var span = new TextSpan(context.Start.StartIndex, context.Stop.StopIndex);
				diagnostics.AddDiagnostic(Diagnostic.ReportInvalidIndexer(context.Start.Line, context.Start.Column, span));

				return null;
			}

			var expressionVisit = Visit(expressionCtx);

			if (expressionVisit == null || !(expressionVisit is ExpressionSyntax expression))
				return null;

			return new VariableIndexerSyntax(Token(identifierCtx.Symbol), Token(lBracket.Symbol), expression, Token(rBracket.Symbol));
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

		private AssignmentExpressionSyntax VisitAssignmentExpression(ITerminalNode identifierCtx, ITerminalNode operatorCtx, GLangParser.AssignmentOperandContext operandCtx, TextLocation location, TextSpan span) {
			if (identifierCtx == null) {
				diagnostics.AddDiagnostic(Diagnostic.ReportMissingToken(location.Line, location.Column, span, "identifier"));
				return null;
			}

			if (operatorCtx == null) {
				diagnostics.AddDiagnostic(Diagnostic.ReportMissingToken(location.Line, location.Column, span, "assignment operator"));
				return null;
			}

			if (operandCtx == null) {
				diagnostics.AddDiagnostic(Diagnostic.ReportMissingToken(location.Line, location.Column, span, "expression"));
				return null;
			}

			var visitOperand = Visit(operandCtx);

			if (visitOperand == null)
				return null;

			if (!(visitOperand is ExpressionSyntax exprSyntax)) {
				var prev = new TextSpan(operatorCtx.Symbol.StartIndex, operatorCtx.Symbol.StopIndex);

				diagnostics.AddDiagnostic(Diagnostic.ReportInvalidAssignmentOperand(operandCtx.Start.Line, operandCtx.Start.Column, prev, visitOperand.Span));
				return null;
			}

			return new AssignmentExpressionSyntax(Token(identifierCtx.Symbol), null, Token(operatorCtx.Symbol), exprSyntax);
		}

		public override SyntaxNode VisitAssignmentExpression([NotNull] GLangParser.AssignmentExpressionContext context) {
			var identifierCtx = context.IDENTIFIER();
			var operatorCtx = context.ASSIGNMENT_OPERATOR();
			var operandCtx = context.assignmentOperand();

			var location = new TextLocation(context.Start.Line, context.Start.Column);
			var span = new TextSpan(context.Start.StartIndex, context.Stop.StopIndex);

			return VisitAssignmentExpression(identifierCtx, operatorCtx, operandCtx, location, span);
		}

		public override SyntaxNode VisitDefinedAssignment([NotNull] GLangParser.DefinedAssignmentContext context) {
			var identifierCtx = context.IDENTIFIER();
			var operatorCtx = context.ASSIGNMENT_OPERATOR();
			var typeDefCtx = context.ASSIGNMENT_OPERATOR();
			var operandCtx = context.assignmentOperand();
			var assignmentCtx = context.assignmentExpression();

			var location = new TextLocation(context.Start.Line, context.Start.Column);
			var span = new TextSpan(context.Start.StartIndex, context.Stop.StopIndex);

			if (assignmentCtx != null)
				return Visit(assignmentCtx);

			if (typeDefCtx == null) {
				diagnostics.AddDiagnostic(Diagnostic.ReportMissingToken(operandCtx.Start.Line, operandCtx.Start.Column, span, "Type definition"));
				return null;
			}

			var visitAssign = VisitAssignmentExpression(identifierCtx, operatorCtx, operandCtx, location, span);
			var visitTypeDef = Visit(typeDefCtx);

			if (visitAssign == null || visitTypeDef == null || !(visitTypeDef is TypeDefinitionSyntax typeDefSyntax))
				return null;

			return new AssignmentExpressionSyntax(visitAssign.IdentifierToken, typeDefSyntax, visitAssign.OperatorToken, visitAssign.Expression);
		}

		public override SyntaxNode VisitVariableDeclarationStatement([NotNull] GLangParser.VariableDeclarationStatementContext context) {
			var keywordCtx = context.DECL_VARIABLE();
			var identifierCtx = context.definedIdentifier();
			var assignmentCtx = context.definedAssignment();

			bool hasDirectDecl = identifierCtx != null;
			bool isAssignmentDecl = assignmentCtx != null;

			if (keywordCtx == null) {
				var span = new TextSpan(context.Start.StartIndex, context.Stop.StopIndex);
				var diagnostic = Diagnostic.ReportMalformedDeclaration(context.Start.Line, context.Start.Column, span);
				diagnostics.AddDiagnostic(diagnostic);
				return null;
			}

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

				if (!(defVisit is TypedIdentifierSyntax def)) {
					diagnostics.AddDiagnostic(Diagnostic.ReportMalformedDeclaration(identifierCtx.Start.Line, identifierCtx.Start.Column, defVisit.Span));
					return null;
				}

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



		public override SyntaxNode VisitExpression([NotNull] GLangParser.ExpressionContext context) {
			var tupleCtx = context.tuple();
			var expressionCtx = context.binaryExpression();

			if (!(OnlyOne(tupleCtx != null, expressionCtx != null))) {
				var span = new TextSpan(context.Start.StartIndex, context.Stop.StopIndex);
				diagnostics.AddDiagnostic(Diagnostic.ReportInvalidExpression(context.Start.Line, context.Start.Column, span));
				return null;
			}

			if (tupleCtx != null)
				return Visit(tupleCtx);

			if (expressionCtx != null)
				return Visit(expressionCtx);

			return null;
		}

		public override SyntaxNode VisitTuple([NotNull] GLangParser.TupleContext context) {
			var lParenCtx = context.L_PARENTHESIS();
			var expressionsCtx = context.seperatedExpression();
			var rParenCtx = context.R_PARENTHESIS();

			bool hasLParen = lParenCtx != null && lParenCtx.GetText().Equals("(");
			bool hasRParen = rParenCtx != null && rParenCtx.GetText().Equals(")");

			if (!hasLParen || expressionsCtx == null || !hasRParen) {
				var span = new TextSpan(context.Start.StartIndex, context.Stop.StopIndex);
				diagnostics.AddDiagnostic(Diagnostic.ReportInvalidTuple(context.Start.Line, context.Start.Column, span));
				return null;
			}

			var seperatedSyntax = InternalVisitSeperatedExpressions(expressionsCtx);

			if (seperatedSyntax == null)
				return null;

			return new TupleSyntax(Token(lParenCtx.Symbol), seperatedSyntax, Token(rParenCtx.Symbol));
		}

		public override SyntaxNode VisitExpressionStatement([NotNull] GLangParser.ExpressionStatementContext context) {
			var assignmentCtx = context.assignmentExpression();
			var expressionCtx = context.expression();

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
			var ifElseCtx = context.ifElseStatement();
			var pureIfCtx = context.pureIfStatement();

			if (!OnlyOne(ifElseCtx != null, pureIfCtx != null)) {
				var span = new TextSpan(context.Start.StartIndex, context.Stop.StopIndex);
				var diagnostic = Diagnostic.ReportInvalidIfStatement(context.Start.Line, context.Start.Column, span);
				diagnostics.AddDiagnostic(diagnostic);
			}

			if (ifElseCtx != null)
				return Visit(ifElseCtx);

			if (pureIfCtx != null)
				return Visit(pureIfCtx);

			return null;
		}

		public override SyntaxNode VisitPureIfStatement([NotNull] GLangParser.PureIfStatementContext context) {
			var ifKeywCtx = context.IF();
			var lParenCtx = context.L_PARENTHESIS();
			var conditionCtx = context.binaryExpression();
			var rParenCtx = context.R_PARENTHESIS();
			var trueCtx = context.trueBranch;

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

			var condVisit = Visit(conditionCtx);
			var trueVisit = Visit(trueCtx);

			if (condVisit == null || trueVisit == null)
				return null;

			if (!(condVisit is ExpressionSyntax condition)) {
				var prev = new TextSpan(lParenCtx.Symbol.StartIndex, lParenCtx.Symbol.StopIndex);
				var next = new TextSpan(rParenCtx.Symbol.StartIndex, rParenCtx.Symbol.StopIndex);

				diagnostics.AddDiagnostic(Diagnostic.ReportInvalidIfStatement(conditionCtx.Start.Line, conditionCtx.Start.Column, prev, condVisit.Span, next));
				return null;
			}

			if (!(trueVisit is StatementSyntax trueStat)) {
				var prev = new TextSpan(rParenCtx.Symbol.StartIndex, rParenCtx.Symbol.StopIndex);

				diagnostics.AddDiagnostic(Diagnostic.ReportInvalidIfStatement(context.Start.Line, context.Start.Column, prev, trueVisit.Span, null));
				return null;
			}

			return new IfStatementSyntax(Token(ifKeywCtx.Symbol), Token(lParenCtx.Symbol), condition, Token(rParenCtx.Symbol), trueStat, null, null);
		}

		public override SyntaxNode VisitIfElseStatement([NotNull] GLangParser.IfElseStatementContext context) {
			var ifCtx = context.pureIfStatement();
			var elseCtx = context.ELSE();
			var falseCtx = context.statement();

			if (ifCtx == null) {
				var span = new TextSpan(context.Start.StartIndex, context.Stop.StopIndex);
				diagnostics.AddDiagnostic(Diagnostic.ReportInvalidIfStatement(context.Start.Line, context.Start.Column, null, span, null));
				return null;
			}

			var ifVisit = Visit(ifCtx);

			if (ifVisit == null || !(ifVisit is IfStatementSyntax ifSyntax))
				return null;

			if (elseCtx == null || !elseCtx.GetText().Equals("else") || falseCtx == null) {
				var prev = new TextSpan(ifCtx.Start.StartIndex, ifCtx.Stop.StopIndex);
				var span = new TextSpan(ifCtx.Stop.StopIndex, context.Stop.StopIndex);
				diagnostics.AddDiagnostic(Diagnostic.ReportInvalidIfStatement(context.Start.Line, context.Start.Column, prev, span, null));
				return null;
			}

			var elseToken = Token(elseCtx.Symbol);
			var falseVisit = Visit(falseCtx);

			if (falseVisit == null || !(falseVisit is StatementSyntax statement))
				return null;

			return new IfStatementSyntax(ifSyntax.IfToken, ifSyntax.LeftParenToken, ifSyntax.Condition, ifSyntax.RightParenToken, ifSyntax.TrueBranch, elseToken, statement);
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

		private SeperatedSyntaxList<ExpressionSyntax> InternalVisitSeperatedExpressions([NotNull] GLangParser.SeperatedExpressionContext context) {
			var builder = ImmutableArray.CreateBuilder<SyntaxNode>();

			if (!InternalVisitSeperatedExpression(context, ref builder))
				return null;

			return new SeperatedSyntaxList<ExpressionSyntax>(builder.ToImmutable());
		}

		private bool InternalVisitSeperatedExpression([NotNull] GLangParser.SeperatedExpressionContext context, ref ImmutableArray<SyntaxNode>.Builder builder) {
			var expressionCtx = context.binaryExpression();
			var commaCtx = context.COMMA();
			var restCtx = context.seperatedExpression();

			bool hasComma = commaCtx != null && commaCtx.GetText().Equals(",");
			bool hasParam = restCtx != null;

			if (expressionCtx == null || (hasComma ^ hasParam)) {
				var span = new TextSpan(context.Start.StartIndex, context.Stop.StopIndex);
				diagnostics.AddDiagnostic(Diagnostic.ReportInvalidCallParameter(expressionCtx.Start.Line, expressionCtx.Start.Column, span));
				return false;
			}

			var visit = Visit(expressionCtx);

			if (visit == null)
				return false;

			if (!(visit is ExpressionSyntax expression)) {
				diagnostics.AddDiagnostic(Diagnostic.ReportInvalidCallParameter(expressionCtx.Start.Line, expressionCtx.Start.Column, visit.Span));
				return false;
			}

			builder.Add(expression);

			if (hasComma && hasParam) {
				builder.Add(Token(commaCtx.Symbol));
				return InternalVisitSeperatedExpression(restCtx, ref builder);
			}

			return true;
		}

		public override SyntaxNode VisitFunctionCall([NotNull] GLangParser.FunctionCallContext context) {
			var funcIdentifierCtx = context.funcName;
			var leftParenCtx = context.L_PARENTHESIS();
			var paramCtx = context.seperatedExpression();
			var rightParenCtx = context.R_PARENTHESIS();

			if (funcIdentifierCtx == null || leftParenCtx == null || rightParenCtx == null
				|| !leftParenCtx.GetText().Equals("(") || !rightParenCtx.GetText().Equals(")")) {
				var span = new TextSpan(context.Start.StartIndex, context.Stop.StopIndex);
				diagnostics.AddDiagnostic(Diagnostic.ReportInvalidFunctionCall(context.Start.Line, context.Start.Column, span));
				return null;
			}

			var parameters = new SeperatedSyntaxList<ExpressionSyntax>(ImmutableArray.Create(new SyntaxNode[] { }));

			if (paramCtx != null) {
				var visit = InternalVisitSeperatedExpressions(paramCtx);

				if (visit == null)
					return null;

				parameters = visit;
			}

			var identToken = Token(funcIdentifierCtx);
			var lParen = Token(leftParenCtx.Symbol);
			var rParen = Token(rightParenCtx.Symbol);

			return new FunctionCallSyntax(identToken, lParen, parameters, rParen);
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
			var returnCtx = context.returnStatement();
			var baseCtx = context.baseStatement();
			var breakCtx = context.BREAK();
			var continueCtx = context.CONTINUE();

			bool hasBreak = breakCtx != null && breakCtx.Symbol.Text.Equals("break");
			bool hasContinue = continueCtx != null && continueCtx.Symbol.Text.Equals("continue");

			if (!OnlyOne(returnCtx != null, baseCtx != null, hasBreak, hasContinue)) {
				var span = new TextSpan(context.Start.StartIndex, context.Stop.StopIndex);
				diagnostics.AddDiagnostic(Diagnostic.ReportInvalidStatement(context.Start.Line, context.Start.Column, span));
				return null;
			}

			if (returnCtx != null)
				return Visit(returnCtx);

			if (baseCtx != null)
				return Visit(baseCtx);

			if (hasBreak)
				return new BreakSyntax(Token(breakCtx.Symbol));

			if (hasContinue)
				return new ContinueSyntax(Token(continueCtx.Symbol));

			return null;
		}

		public override SyntaxNode VisitReturnStatement([NotNull] GLangParser.ReturnStatementContext context) {
			var keywCtx = context.RETURN();
			var expressionCtx = context.binaryExpression();

			if (keywCtx == null || !keywCtx.GetText().Equals("return")) {
				var span = new TextSpan(context.Start.StartIndex, context.Stop.StopIndex);
				diagnostics.AddDiagnostic(Diagnostic.ReportInvalidReturnStatement(context.Start.Line, context.Start.Column, span));
				return null;
			}

			ExpressionSyntax expression = null;

			if (expressionCtx != null) {
				var visit = Visit(expressionCtx);

				if (visit == null)
					return null;

				if (!(visit is ExpressionSyntax syntax)) {
					var span = new TextSpan(context.Start.StartIndex, context.Stop.StopIndex);
					diagnostics.AddDiagnostic(Diagnostic.ReportInvalidReturnStatement(context.Start.Line, context.Start.Column, span));
					return null;
				}

				expression = syntax;
			}

			return new ReturnSyntax(Token(keywCtx.Symbol), expression);
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

		private SeperatedSyntaxList<TypedIdentifierSyntax> InternalVisitParameterDefinition([NotNull] GLangParser.ParameterDefinitionContext context) {
			var builder = ImmutableArray.CreateBuilder<SyntaxNode>();

			if (!InternalVisitParameterDefinition(context, ref builder))
				return null;

			return new SeperatedSyntaxList<TypedIdentifierSyntax>(builder.ToImmutable());
		}

		private bool InternalVisitParameterDefinition([NotNull] GLangParser.ParameterDefinitionContext context, ref ImmutableArray<SyntaxNode>.Builder builder) {
			var typeDefCtx = context.definedIdentifier();
			var commaCtx = context.COMMA();
			var paramCtx = context.parameterDefinition();

			var hasComma = commaCtx != null && commaCtx.GetText().Equals(",");
			var hasParam = paramCtx != null;

			if (typeDefCtx == null || (hasComma ^ hasParam)) {
				var span = new TextSpan(context.Start.StartIndex, context.Stop.StopIndex);
				var diagnostic = Diagnostic.ReportInvalidParameterDefinition(context.Start.Line, context.Start.Column, span);
				diagnostics.AddDiagnostic(diagnostic);
				return false;
			}

			var visit = Visit(typeDefCtx);

			if (visit == null)
				return false;

			builder.Add(visit);

			if (hasComma && hasParam) {
				builder.Add(Token(commaCtx.Symbol));
				return InternalVisitParameterDefinition(paramCtx, ref builder);
			}

			return true;
		}

		public override SyntaxNode VisitFunctionDefinition([NotNull] GLangParser.FunctionDefinitionContext context) {
			var keywCtx = context.FUNCTION();
			var identCtx = context.IDENTIFIER();
			var lParenCtx = context.L_PARENTHESIS();
			var paramCtx = context.parameterDefinition();
			var rParenCtx = context.R_PARENTHESIS();
			var typeDefCtx = context.typeDefinition();
			var bodyCtx = context.block();

			if (keywCtx == null || identCtx == null || lParenCtx == null || rParenCtx == null || typeDefCtx == null || bodyCtx == null
				|| !lParenCtx.GetText().Equals("(") || !rParenCtx.GetText().Equals(")") || !keywCtx.GetText().Equals("function")) {
				var span = new TextSpan(context.Start.StartIndex, context.Stop.StopIndex);
				diagnostics.AddDiagnostic(Diagnostic.ReportInvalidFunctionDef(context.Start.Line, context.Start.Column, span));
				return null;
			}

			var paramVisit = new SeperatedSyntaxList<TypedIdentifierSyntax>(ImmutableArray.Create(new SyntaxNode[] { }));
			var typeDefVisit = Visit(typeDefCtx);
			var bodyVisit = Visit(bodyCtx);

			if (typeDefVisit == null || bodyVisit == null)
				return null;

			if (paramCtx != null) {
				var visit = InternalVisitParameterDefinition(paramCtx);

				if (visit == null)
					return null;

				paramVisit = visit;
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
			var lParenToken = Token(lParenCtx.Symbol);
			var rParenToken = Token(rParenCtx.Symbol);

			var declSyntax = new FunctionDeclarationSyntax(keywToken, identToken, lParenToken, paramVisit, rParenToken, typeDef, body);

			return declSyntax;
		}
	}
}
