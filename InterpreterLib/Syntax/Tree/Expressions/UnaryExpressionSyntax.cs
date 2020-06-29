using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Syntax.Tree.Expressions {
	internal sealed class UnaryExpressionSyntax : ExpressionSyntax {
		public override SyntaxType Type => SyntaxType.UnaryExpression;
		public override IEnumerable<SyntaxNode> Children {
			get {
				yield return OpToken;
				yield return Expression;
			}
		}

		public TokenSyntax OpToken { get; }
		public ExpressionSyntax Expression { get; }
		public override TextSpan Span { get; }

		public UnaryExpressionSyntax(TokenSyntax opToken, ExpressionSyntax expression) {
			OpToken = opToken;
			Expression = expression;

			Span = CreateNewSpan(OpToken.Span, Expression.Span);
		}

		public override string ToString() {
			var builder = new StringBuilder();

			builder.Append(OpToken).Append(" ");
			builder.Append(Expression);

			return builder.ToString();
		}
	}
}
