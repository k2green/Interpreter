using InterpreterLib.Symbols.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Symbols.Binding {
	public class VariableSymbol<T> : Symbol where T : TypeSymbol {

		public override string Name { get; }
		public bool IsReadOnly { get; }
		public T ValueType { get; }

		public override SymbolType Type => SymbolType.Variable;

		public VariableSymbol(string name, bool isReadOnly, T valueType) {
			Name = name;
			IsReadOnly = isReadOnly;
			ValueType = valueType;
		}

		public override string ToString() => $"{Name} : {ValueType}";

		public override bool Equals(object obj) {
			if (!(obj is VariableSymbol<T> symbol)) return false;

			return Name.Equals(symbol.Name) && IsReadOnly == symbol.IsReadOnly && ValueType.Equals(symbol.ValueType);
		}

		public override int GetHashCode() {
			return HashCode.Combine(Name, IsReadOnly, ValueType);
		}
	}
}
