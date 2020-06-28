using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Syntax.Tree.Expressions {
	internal sealed class BinaryExpressionSyntax : ExpressionSyntax {
		public override SyntaxType Type => SyntaxType.BinaryExpression;
		public override IEnumerable<SyntaxNode> Children {
			get {
				yield return LeftSyntax;
				yield return OpToken;
				yield return RightSyntax;
			}
		}

		public ExpressionSyntax LeftSyntax { get; }
		public TokenSyntax OpToken { get; }
		public ExpressionSyntax RightSyntax { get; }

		public BinaryExpressionSyntax(ExpressionSyntax leftStntax, TokenSyntax opToken, ExpressionSyntax rightSyntax) {
			LeftSyntax = leftStntax;
			OpToken = opToken;
			RightSyntax = rightSyntax;
		}
	}
}
