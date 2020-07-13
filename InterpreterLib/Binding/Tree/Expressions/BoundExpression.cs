
using InterpreterLib.Symbols.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Binding.Tree.Expressions {
	internal abstract class BoundExpression : BoundNode {
		public abstract ValueTypeSymbol ValueType { get; }
	}
}
