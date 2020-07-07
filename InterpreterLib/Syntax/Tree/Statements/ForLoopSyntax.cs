using InterpreterLib.Syntax.Tree.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Syntax.Tree.Statements {
	internal sealed class ForLoopSyntax : StatementSyntax {
		public override SyntaxType Type => SyntaxType.ForLoop;
		public override TextLocation Span { get; }

		public override IEnumerable<SyntaxNode> Children {
			get {
				yield return ForToken;
				yield return LeftParenToken;
				yield return Assignment;
				yield return Comma1;
				yield return Condition;
				yield return Comma2;
				yield return Step;
				yield return RightParenToken;
				yield return Body;
			}
		}

		public TokenSyntax ForToken { get; }
		public TokenSyntax LeftParenToken { get; }
		public StatementSyntax Assignment { get; }
		public TokenSyntax Comma1 { get; }
		public ExpressionSyntax Condition { get; }
		public TokenSyntax Comma2 { get; }
		public ExpressionSyntax Step { get; }
		public TokenSyntax RightParenToken { get; }
		public StatementSyntax Body { get; }

		public ForLoopSyntax(

			TokenSyntax forToken,
			TokenSyntax leftParenToken,
			StatementSyntax assignment,
			TokenSyntax comma1,
			ExpressionSyntax condition,
			TokenSyntax comma2,
			ExpressionSyntax step,
			TokenSyntax rightParenToken,
			StatementSyntax body 
			
			)
		{

			ForToken = forToken;
			LeftParenToken = leftParenToken;
			Assignment = assignment;
			Comma1 = comma1;
			Condition = condition;
			Comma2 = comma2;
			Step = step;
			RightParenToken = rightParenToken;
			Body = body;

			Span = CreateNewSpan(ForToken.Span, Body.Span);
		}

		public override string ToString() {
			var builder = new StringBuilder();

			builder.Append(ForToken).Append(" ");
			builder.Append(LeftParenToken);
			builder.Append(Assignment).Append(Comma1).Append(" ");
			builder.Append(Condition).Append(Comma2).Append(" ");
			builder.Append(Step);
			builder.Append(RightParenToken).Append(" ");
			builder.Append(Body);

			return builder.ToString();
		}
	}
}
