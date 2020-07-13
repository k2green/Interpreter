using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Symbols.Binding {
	public class LabelSymbol : Symbol {

		public override SymbolType Type => SymbolType.Label;
		public override string Name { get; }

		public LabelSymbol(string name) {
			Name = name;
		}

		public override bool Equals(object obj) {
			if (!(obj is LabelSymbol symbol))
				return false;

			return Name.Equals(symbol.Name);
		}

		public override int GetHashCode() {
			return Name.GetHashCode();
		}
	}
}
