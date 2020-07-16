using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Syntax.Tree.Expressions {
	internal sealed class TupleSyntax : ExpressionSyntax {
		public override SyntaxType Type => SyntaxType.Tuple;

		public override IEnumerable<SyntaxNode> Children => throw new NotImplementedException();

		public override TextLocation Location => LeftParen.Location;
		public override TextSpan Span => new TextSpan(LeftParen.Span.Start, RightParen.Span.End);

		public TokenSyntax LeftParen { get; }
		public SeperatedSyntaxList<ExpressionSyntax> Items { get; }
		public TokenSyntax RightParen { get; }

		public TupleSyntax(TokenSyntax leftParen, SeperatedSyntaxList<ExpressionSyntax> items, TokenSyntax rightParen) {
			LeftParen = leftParen;
			Items = items;
			RightParen = rightParen;
		}
	}
}
