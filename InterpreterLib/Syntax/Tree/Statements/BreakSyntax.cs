using InterpreterLib.Syntax.Tree.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Syntax.Tree.Statements {
	internal sealed class BreakSyntax : StatementSyntax {
		public override SyntaxType Type => SyntaxType.Break;

		public override TextLocation Location => BreakToken.Location;
		public override TextSpan Span => BreakToken.Span;

		public override IEnumerable<SyntaxNode> Children {
			get {
				yield return BreakToken;
			}
		}

		public TokenSyntax BreakToken { get; }

		public BreakSyntax(TokenSyntax breakToken) {
			BreakToken = breakToken;
		}
	}
}
