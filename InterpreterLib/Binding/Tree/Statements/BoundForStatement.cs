using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Binding.Tree {
	internal sealed class BoundForStatement : BoundStatement {

		public override NodeType Type => NodeType.For;

		public BoundNode Assignment { get; }
		public BoundExpression Condition { get; }
		public BoundExpression Step { get; }
		public BoundStatement Body { get; }

		public BoundForStatement(BoundNode assignment, BoundExpression condition, BoundExpression step, BoundStatement body) {
			Assignment = assignment;
			Condition = condition;
			Step = step;
			Body = body;
		}
	}
}
