using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Syntax.Tree.Expressions {
	internal sealed class VariableSyntax : ExpressionSyntax {
		public override SyntaxType Type => SyntaxType.Variable;

		public override IEnumerable<SyntaxNode> Children { get { yield return IdentifierToken; } }

		public TokenSyntax IdentifierToken { get; }

		public VariableSyntax(TokenSyntax identifierToken) {
			IdentifierToken = identifierToken;
		}
	}
}
