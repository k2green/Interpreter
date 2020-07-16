using InterpreterLib.Symbols.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Symbols.Binding {
	public sealed class ParameterSymbol : Symbol {

		public override SymbolType Type => SymbolType.Parameter;

		public override string Name { get; }
		public TypeSymbol ValueType { get; }

		public ParameterSymbol(string name, TypeSymbol valueType) {
			Name = name;
			ValueType = valueType;
		}

		public override bool Equals(object obj) {
			if (!(obj is ParameterSymbol)) return false;

			ParameterSymbol symbol = (ParameterSymbol)obj;
			return Name.Equals(symbol.Name) && ValueType == symbol.ValueType;
		}

		public override int GetHashCode() {
			return HashCode.Combine(Name, ValueType);
		}
	}
}
