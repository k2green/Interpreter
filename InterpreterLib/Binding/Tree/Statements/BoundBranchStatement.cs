using InterpreterLib.Binding.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Binding.Tree.Statements {
	internal sealed class BoundBranchStatement : BoundStatement {

		public override NodeType Type => NodeType.Branch;
		public LabelSymbol Label { get; }

		public BoundBranchStatement(LabelSymbol label) {
			Label = label;
		}
	}
}
