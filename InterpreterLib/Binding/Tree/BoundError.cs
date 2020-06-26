using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Binding.Tree {
	internal sealed class BoundError : BoundNode {

		public Diagnostic Error { get; }
		public override NodeType Type => NodeType.Error;

		public BoundError(Diagnostic error) {
			Error = error;
		}
	}
}
