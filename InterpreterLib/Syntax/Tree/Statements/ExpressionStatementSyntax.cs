using InterpreterLib.Syntax.Tree.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Syntax.Tree.Statements {
	internal sealed class ExpressionStatementSyntax : StatementSyntax {
		public override SyntaxType Type => SyntaxType.Expression;

		public override IEnumerable<SyntaxNode> Children { get { yield return Expression; } }

		public ExpressionSyntax Expression { get; }

		public ExpressionStatementSyntax(ExpressionSyntax expression) {
			Expression = expression;
		}
	}
}
