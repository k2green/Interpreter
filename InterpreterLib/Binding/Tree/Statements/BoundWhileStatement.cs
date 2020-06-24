using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Binding.Tree {
	internal sealed class BoundWhileStatement : BoundNode {

		public override NodeType Type => NodeType.While;

		public BoundExpression Condition { get; }
		public BoundNode Body { get; }

		public BoundWhileStatement(BoundExpression condition, BoundNode body) {
			Condition = condition;
			Body = body;
		}
	}
}
