using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Binding.Tree {
	internal sealed class BoundConditionalBranchStatement : BoundStatement {

		public override NodeType Type => NodeType.ConditionalBranch;
		public BoundLabel Label { get; }
		public BoundExpression Condition { get; }
		public bool Check { get; }

		public BoundConditionalBranchStatement(BoundLabel label, BoundExpression condition, bool check) {
			Label = label;
			Condition = condition;
			Check = check;
		}
	}
}
