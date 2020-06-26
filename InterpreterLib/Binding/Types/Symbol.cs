using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Binding.Types {
	public abstract class Symbol {

		public abstract SymbolType Type { get; }
		public abstract string Name { get; }

		public override string ToString() => Name;
	}
}
