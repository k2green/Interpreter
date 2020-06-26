using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Binding.Tree {
	internal sealed class BoundForStatement : BoundNode {

		public override NodeType Type => NodeType.For;

		public BoundExpression Assignment { get; }
		public BoundWhileStatement While { get; }

		public BoundForStatement(BoundExpression assignment, BoundWhileStatement whileStat) {
			Assignment = assignment;
			While = whileStat;
		}
	}
}
