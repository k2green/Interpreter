using InterpreterLib.Syntax.Tree.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Syntax.Tree.Statements {
	internal sealed class VariableDeclarationSyntax : StatementSyntax {

		public override SyntaxType Type => SyntaxType.Declaration;
		public override TextSpan Span { get; }

		public override IEnumerable<SyntaxNode> Children {
			get {
				yield return KeywordToken;
				yield return IdentifierToken;
				yield return Definition;
				yield return OperatorToken;
				yield return Initialiser;
			}
		}

		public TokenSyntax KeywordToken { get; }
		public TokenSyntax IdentifierToken { get; }
		public TypeDefinitionSyntax Definition { get; }
		public TokenSyntax OperatorToken { get; }
		public ExpressionSyntax Initialiser { get; }


		public VariableDeclarationSyntax(TokenSyntax keywordToken, TokenSyntax identifierToken, TypeDefinitionSyntax definition, TokenSyntax operatorToken, ExpressionSyntax initialiser) {
			KeywordToken = keywordToken;
			IdentifierToken = identifierToken;
			Definition = definition;
			OperatorToken = operatorToken;
			Initialiser = initialiser;

			if (initialiser != null)
				Span = CreateNewSpan(keywordToken.Span, initialiser.Span);
			else
				Span = CreateNewSpan(keywordToken.Span, definition.Span);
		}
	}
}
