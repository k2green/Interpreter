using System;
using System.Collections.Generic;
using System.Text;
using InterpreterLib.Binding.Tree.Expressions;
using InterpreterLib.Types;

namespace InterpreterLib.Binding.Tree.Statements {
	internal sealed class BoundExpressionStatement : BoundStatement {

		public override NodeType Type => NodeType.Expression;
		public BoundExpression Expression { get; }

		public BoundExpressionStatement(BoundExpression expression) {
			Expression = expression;
		}

		public override string ToString() => Expression.ToString();
	}
}
