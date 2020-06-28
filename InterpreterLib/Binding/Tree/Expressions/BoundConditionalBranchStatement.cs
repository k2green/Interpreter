using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Binding.Tree.Expressions {
	internal sealed class BoundConditionalBranchStatement : BoundStatement {

		public override NodeType Type => NodeType.ConditionalBranch;
		public BoundLabel Label { get; }
		public BoundExpression Condition { get; }
		public bool IsFalse { get; }

		public BoundConditionalBranchStatement(BoundLabel label, BoundExpression condition, bool isFalse) {
			Label = label;
			Condition = condition;
			IsFalse = isFalse;
		}
	}
}
