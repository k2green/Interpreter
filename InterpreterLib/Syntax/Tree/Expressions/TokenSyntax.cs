using Antlr4.Runtime;
using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Syntax.Tree.Expressions {
	internal sealed class TokenSyntax : ExpressionSyntax {
		public override IEnumerable<SyntaxNode> Children => throw new Exception("Tokens have no children");
		public override SyntaxType Type => SyntaxType.Token;

		public IToken Token { get; }
		public override TextLocation Location => new TextLocation(Token.Line, Token.Column);
		public override TextSpan Span => new TextSpan(Token.StartIndex, Token.StopIndex);

		public TokenSyntax(IToken token) {
			Token = token;
		}

		public override string ToString() {
			return Token.Text;
		}
	}
}
