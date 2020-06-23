using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Binding.Tree {
	internal abstract class BoundExpression : BoundNode {
		public abstract Type ValueType { get; }
	}
}
