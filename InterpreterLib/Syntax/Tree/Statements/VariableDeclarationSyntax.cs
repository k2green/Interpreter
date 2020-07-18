using InterpreterLib.Syntax.Tree.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Syntax.Tree.Statements {
	internal sealed class VariableDeclarationSyntax : StatementSyntax {

		public override SyntaxType Type => SyntaxType.VariableDeclaration;
		public override TextLocation Location => KeywordToken.Location;
		public override TextSpan Span => new TextSpan(KeywordToken.Span.Start, Initialiser.Span.End);

		public override IEnumerable<SyntaxNode> Children {
			get {
				yield return KeywordToken;
				yield return Identifier;
				yield return Initialiser;
			}
		}

		public TokenSyntax KeywordToken { get; }
		public TypedIdentifierSyntax Identifier { get; }
		public AssignmentExpressionSyntax Initialiser { get; }

		public VariableDeclarationSyntax(TokenSyntax keywordToken, TypedIdentifierSyntax identifier, AssignmentExpressionSyntax initialiser) {
			KeywordToken = keywordToken;
			Identifier = identifier;
			Initialiser = initialiser;
		}
	}
}
