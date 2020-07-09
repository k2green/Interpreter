using InterpreterLib.Syntax.Tree.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Syntax.Tree.Statements {
	internal sealed class IfStatementSyntax : StatementSyntax {
		public override SyntaxType Type => SyntaxType.IfStatement;
		public override TextLocation Location => IfToken.Location;
		public override TextSpan Span { get; }

		public override IEnumerable<SyntaxNode> Children {
			get {
				yield return IfToken;
				yield return LeftParenToken;
				yield return Condition;
				yield return RightParenToken;
				yield return TrueBranch;
				yield return ElseToken;
				yield return FalseBranch;
			}
		}

		public TokenSyntax IfToken { get; }
		public TokenSyntax LeftParenToken { get; }
		public ExpressionSyntax Condition { get; }
		public TokenSyntax RightParenToken { get; }
		public StatementSyntax TrueBranch { get; }
		public TokenSyntax ElseToken { get; }
		public StatementSyntax FalseBranch { get; }

		public IfStatementSyntax(TokenSyntax ifToken, TokenSyntax leftParenToken, ExpressionSyntax condition, TokenSyntax rightParenToken, StatementSyntax trueBranch, TokenSyntax elseToken, StatementSyntax falseBranch) {
			IfToken = ifToken;
			LeftParenToken = leftParenToken;
			Condition = condition;
			RightParenToken = rightParenToken;
			TrueBranch = trueBranch;
			ElseToken = elseToken;
			FalseBranch = falseBranch;

			if (ElseToken == null && FalseBranch == null)
				Span = new TextSpan(IfToken.Span.Start, TrueBranch.Span.End);
			else
				Span = new TextSpan(IfToken.Span.Start, FalseBranch.Span.End);
		}
	}
}
