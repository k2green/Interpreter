using InterpreterLib.Syntax.Tree.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Syntax.Tree.Statements {
	internal sealed class WhileLoopSyntax : StatementSyntax {
		public override SyntaxType Type => SyntaxType.WhileLoop;
		public override TextSpan Span { get; }

		public override IEnumerable<SyntaxNode> Children {
			get {
				yield return WhileToken;
				yield return LeftParenToken;
				yield return Condition;
				yield return RightParenToken;
				yield return Body;
			}
		}

		public TokenSyntax WhileToken { get; }
		public TokenSyntax LeftParenToken { get; }
		public ExpressionSyntax Condition { get; }
		public TokenSyntax RightParenToken { get; }
		public StatementSyntax Body { get; }

		public WhileLoopSyntax(TokenSyntax whileToken, TokenSyntax leftParenToken, ExpressionSyntax condition, TokenSyntax rightParenToken, StatementSyntax body) {
			WhileToken = whileToken;
			LeftParenToken = leftParenToken;
			Condition = condition;
			RightParenToken = rightParenToken;
			Body = body;

			Span = CreateNewSpan(whileToken.Span, body.Span);
		}

		public override string ToString() {
			var builder = new StringBuilder();

			builder.Append(WhileToken).Append(" ");
			builder.Append(LeftParenToken);
			builder.Append(Condition);
			builder.Append(RightParenToken).Append(" ");
			builder.Append(Body);

			return builder.ToString();
		}
	}
}
