using InterpreterLib.Syntax.Tree.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Syntax.Tree.Statements {
	internal sealed class WhileLoopSyntax : StatementSyntax {
		public override SyntaxType Type => SyntaxType.WhileLoop;
		public override TextLocation Location => WhileToken.Location;
		public override TextSpan Span => new TextSpan(WhileToken.Span.Start, Body.Span.End);

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
		}
	}
}
