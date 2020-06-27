using System;
using System.Collections.Generic;
using System.Text;
using InterpreterLib.Binding.Types;

namespace InterpreterLib.Binding.Tree {
	internal sealed class BoundExpressionStatement : BoundStatement {

		public override NodeType Type => NodeType.Expression;
		public BoundExpression Expression { get; }

		public BoundExpressionStatement(BoundExpression expression) {
			Expression = expression;
		}
	}
}
