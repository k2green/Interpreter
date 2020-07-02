using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Binding.Tree.Statements {
	internal sealed class BoundBranchStatement : BoundStatement {

		public override NodeType Type => NodeType.Branch;
		public BoundLabel Label { get; }

		public BoundBranchStatement(BoundLabel label) {
			Label = label;
		}
	}
}
