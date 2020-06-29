using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Syntax.Tree.Expressions {
	internal sealed class VariableSyntax : ExpressionSyntax {
		public override SyntaxType Type => SyntaxType.Variable;

		public override IEnumerable<SyntaxNode> Children { get { yield return IdentifierToken; } }

		public TokenSyntax IdentifierToken { get; }
		public override TextSpan Span { get; }

		public VariableSyntax(TokenSyntax identifierToken) {
			IdentifierToken = identifierToken;
			Span = IdentifierToken.Span;
		}

		public override string ToString() {
			return IdentifierToken.ToString();
		}
	}
}
