using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Binding.Tree {
	internal sealed class BoundWhileStatement : BoundStatement {

		public override NodeType Type => NodeType.While;

		public BoundExpression Condition { get; }
		public BoundStatement Body { get; }

		public BoundWhileStatement(BoundExpression condition, BoundStatement body) {
			Condition = condition;
			Body = body;
		}
	}
}
