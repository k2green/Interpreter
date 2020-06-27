using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Binding.Tree {
	internal sealed class BoundForStatement : BoundStatement {

		public override NodeType Type => NodeType.For;

		public BoundExpression Assignment { get; }
		public BoundExpression Condition { get; }
		public BoundExpression Step { get; }
		public BoundNode Body { get; }

		public BoundForStatement(BoundExpression assignment, BoundExpression condition, BoundExpression step, BoundNode body) {
			Assignment = assignment;
			Condition = condition;
			Step = step;
			Body = body;
		}
	}
}
