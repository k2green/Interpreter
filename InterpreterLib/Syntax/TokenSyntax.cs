using Antlr4.Runtime;
using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Syntax.Tree.Expressions {
	internal sealed class TokenSyntax : SyntaxNode {
		public override IEnumerable<SyntaxNode> Children => throw new Exception("Tokens have no children");
		public override SyntaxType Type => SyntaxType.Token;

		public IToken Token { get; }

		public TokenSyntax(IToken token) {
			Token = token;
		}
	}
}
