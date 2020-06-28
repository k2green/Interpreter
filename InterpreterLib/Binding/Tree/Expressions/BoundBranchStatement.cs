using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Binding.Tree.Expressions {
	internal sealed class BoundBranchStatement : BoundStatement {

		public override NodeType Type => NodeType.ConditionalBranch;
		public BoundLabel Label { get; }

		public BoundBranchStatement(BoundLabel label) {
			Label = label;
		}
	}
}
