using Antlr4.Runtime;
using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Syntax.Tree {
	internal sealed class TokenSyntax : SyntaxNode {
		public override IEnumerable<SyntaxNode> Children => throw new Exception("Tokens have no children");
		public override SyntaxType Type => SyntaxType.Token;

		public IToken Token { get; }
		public override TextSpan Span { get; }

		public TokenSyntax(IToken token) {
			Token = token;
			Span = new TextSpan(token.StartIndex, token.Text.Length, token.StartIndex, token.Column);
		}

		public override string ToString() {
			return Token.Text;
		}
	}
}
