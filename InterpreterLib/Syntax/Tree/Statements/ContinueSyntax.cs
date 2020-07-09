using InterpreterLib.Syntax.Tree.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Syntax.Tree.Statements {
	internal sealed class ContinueSyntax : StatementSyntax {
		public override SyntaxType Type => SyntaxType.Continue;

		public override TextLocation Location => ContinueToken.Location;
		public override TextSpan Span => ContinueToken.Span;

		public override IEnumerable<SyntaxNode> Children {
			get {
				yield return ContinueToken;
			}
		}

		public TokenSyntax ContinueToken { get; }

		public ContinueSyntax(TokenSyntax continueToken) {
			ContinueToken = continueToken;
		}
	}
}