using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Syntax.Tree.Expressions {
	internal class ArraySyntax : ExpressionSyntax {
		public override SyntaxType Type => SyntaxType.Array;

		public override IEnumerable<SyntaxNode> Children {
			get {
				yield return ArrayToken;
				yield return LeftParen;

				foreach (var val in Values)
					yield return val;

				yield return RightParen;
			}
		}

		public TokenSyntax ArrayToken { get; }
		public TokenSyntax LeftParen { get; }
		public SeperatedSyntaxList<ExpressionSyntax> Values { get; }
		public TokenSyntax RightParen { get; }

		public ArraySyntax(TokenSyntax arrayToken, TokenSyntax leftParen, SeperatedSyntaxList<ExpressionSyntax> values, TokenSyntax rightParen) {
			ArrayToken = arrayToken;
			LeftParen = leftParen;
			Values = values;
			RightParen = rightParen;
		}
	}
}
