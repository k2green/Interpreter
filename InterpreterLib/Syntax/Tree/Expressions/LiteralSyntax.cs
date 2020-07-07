using Antlr4.Runtime;
using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Syntax.Tree.Expressions {
	internal sealed class LiteralSyntax : ExpressionSyntax {

		public override SyntaxType Type => SyntaxType.Literal;
		public override IEnumerable<SyntaxNode> Children {
			get {
				yield return LiteralToken;
			}
		}

		public TokenSyntax LiteralToken { get; }
		public override TextLocation Span { get; }

		public LiteralSyntax(TokenSyntax literalToken) {
			LiteralToken = literalToken;

			Span = literalToken.Span;
		}

		public override string ToString() {
			return LiteralToken.ToString();
		}
	}
}
