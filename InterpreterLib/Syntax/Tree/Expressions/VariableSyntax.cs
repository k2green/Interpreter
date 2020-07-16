using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Syntax.Tree.Expressions {
	internal sealed class VariableSyntax : AccessorExpressionSyntax {
		public override SyntaxType Type => SyntaxType.Variable;

		public override IEnumerable<SyntaxNode> Children { get { yield return IdentifierToken; } }

		public TokenSyntax IdentifierToken { get; }
		public override TextLocation Location => IdentifierToken.Location;
		public override TextSpan Span => IdentifierToken.Span;

		public VariableSyntax(TokenSyntax identifierToken) {
			IdentifierToken = identifierToken;
		}

		public override string ToString() {
			return IdentifierToken.ToString();
		}
	}
}
