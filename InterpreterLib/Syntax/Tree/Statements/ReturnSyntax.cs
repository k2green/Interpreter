using InterpreterLib.Syntax.Tree.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Syntax.Tree.Statements {
	internal sealed class ReturnSyntax : StatementSyntax {
		public override SyntaxType Type => SyntaxType.Return;

		public override TextLocation Location => ReturnToken.Location;
		public override TextSpan Span => Expression != null ? new TextSpan(ReturnToken.Span.Start, Expression.Span.End) : ReturnToken.Span;

		public override IEnumerable<SyntaxNode> Children {
			get {
				yield return ReturnToken;
				yield return Expression;
			}
		}

		public TokenSyntax ReturnToken { get; }
		public ExpressionSyntax Expression { get; }

		public ReturnSyntax(TokenSyntax returnToken, ExpressionSyntax expression) {
			ReturnToken = returnToken;
			Expression = expression;
		}
	}
}
