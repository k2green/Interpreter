using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Binding.Tree {
	internal abstract class BoundNode {
		public abstract NodeType Type { get; }
	}
}
