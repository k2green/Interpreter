using InterpreterLib.Symbols.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Symbols.Binding {
	public class VariableSymbol : VariableSymbol<TypeSymbol>, ICovariant<TypeSymbol> {
		public VariableSymbol(string name, bool isReadOnly, TypeSymbol variableSymbol) : base(name, isReadOnly, variableSymbol) {

		}

		public VariableSymbol(string name, bool isReadOnly, ICovariant<TypeSymbol> variableSymbol) : base(name, isReadOnly, variableSymbol.ValueType) {

		}
	}

	public class VariableSymbol<T> : Symbol, ICovariant<T> where T : TypeSymbol {
		public override string Name { get; }
		public bool IsReadOnly { get; }
		public T ValueType { get; }

		public override SymbolType Type => SymbolType.Variable;

		protected VariableSymbol(string name, bool isReadOnly) {
			Name = name;
			IsReadOnly = isReadOnly;
		}

		public VariableSymbol(string name, bool isReadOnly, T valueType) : this(name, isReadOnly) {
			ValueType = valueType;
		}

		public VariableSymbol BaseSymbol => new VariableSymbol(Name, IsReadOnly, this);

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
