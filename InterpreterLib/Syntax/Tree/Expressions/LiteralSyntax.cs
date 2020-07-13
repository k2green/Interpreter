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
		public object Value { get; }

		public override TextLocation Location => LiteralToken.Location;
		public override TextSpan Span => LiteralToken.Span;

		public LiteralSyntax(TokenSyntax literalToken, object value) {
			LiteralToken = literalToken;
			Value = value;
		}

		public override string ToString() {
			return LiteralToken.ToString();
		}
	}
}
