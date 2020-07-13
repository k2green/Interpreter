using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Symbols {
	public abstract class Symbol {

		public abstract SymbolType Type { get; }
		public abstract string Name { get; }

		public override string ToString() => Name;
		public abstract override bool Equals(object obj);
		public abstract override int GetHashCode();
	}
}
