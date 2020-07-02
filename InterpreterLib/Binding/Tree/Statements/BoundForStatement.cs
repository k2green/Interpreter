using InterpreterLib.Binding.Tree.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Binding.Tree.Statements {
	internal sealed class BoundForStatement : BoundStatement {

		public override NodeType Type => NodeType.For;

		public BoundStatement Assignment { get; }
		public BoundExpression Condition { get; }
		public BoundExpression Step { get; }
		public BoundStatement Body { get; }

		public BoundForStatement(BoundStatement assignment, BoundExpression condition, BoundExpression step, BoundStatement body) {
			Assignment = assignment;
			Condition = condition;
			Step = step;
			Body = body;
		}
	}
}
