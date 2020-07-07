using Antlr4.Runtime;
using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Syntax.Tree.Expressions {
	internal sealed class TokenSyntax : ExpressionSyntax {
		public override IEnumerable<SyntaxNode> Children => throw new Exception("Tokens have no children");
		public override SyntaxType Type => SyntaxType.Token;

		public IToken Token { get; }
		public override TextLocation Span { get; }

		public TokenSyntax(IToken token) {
			Token = token;
			Span = new TextLocation(token.StartIndex, token.Text.Length, token.StartIndex, token.Column);
		}

		public override string ToString() {
			return Token.Text;
		}
	}
}
