using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Syntax.Tree.Expressions {
	internal sealed class VariableIndexerSyntax : AccessorExpressionSyntax {
		public override SyntaxType Type => SyntaxType.VariableIndexer;
		public override TextLocation Location => Item.Location;
		public override TextSpan Span => new TextSpan(Item.Span.Start, RightBracket.Span.End);

		public override IEnumerable<SyntaxNode> Children {
			get {
				yield return Item;
				yield return LeftBracket;
				yield return Expression;
				yield return RightBracket;
			}
		}

		public ExpressionSyntax Item { get; }
		public TokenSyntax LeftBracket { get; }
		public ExpressionSyntax Expression { get; }
		public TokenSyntax RightBracket { get; }

		public VariableIndexerSyntax(ExpressionSyntax item, TokenSyntax leftBracket, ExpressionSyntax expression, TokenSyntax rightBracket) {
			Item = item;
			LeftBracket = leftBracket;
			Expression = expression;
			RightBracket = rightBracket;
		}
	}
}
