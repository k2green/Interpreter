using InterpreterLib.Symbols.Binding;
using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Symbols.Types {
	internal abstract class AccessibleSymbol : TypeSymbol {

		public abstract IReadOnlyList<VariableSymbol> Variables { get; }
	}
}
