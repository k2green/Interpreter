using InterpreterLib.Binding;
using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Symbols.Types {
	internal abstract class AccessibleSymbol : TypeSymbol {

		public abstract BoundScope Scope { get; }
	}
}
