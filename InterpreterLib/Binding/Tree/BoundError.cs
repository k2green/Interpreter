using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Binding.Tree {
	internal sealed class BoundError : BoundNode {

		public IEnumerable<BoundNode> Children { get; }
		public Diagnostic Error { get; }
		public bool IsCausing { get; }

		public override NodeType Type => NodeType.Error;

		public BoundError(Diagnostic error, bool isCausing, params BoundNode[] children) {
			Error = error;
			IsCausing = isCausing;
			Children = children;
		}
	}
}
