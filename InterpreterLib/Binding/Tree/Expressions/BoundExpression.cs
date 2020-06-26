using InterpreterLib.Binding.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Binding.Tree {
	internal abstract class BoundExpression : BoundNode {
		public abstract TypeSymbol ValueType { get; }
	}
}
