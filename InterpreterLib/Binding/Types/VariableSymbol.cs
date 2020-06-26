using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Binding.Types {
	public class VariableSymbol : Symbol {

		public override string Name { get; }
		public bool IsReadOnly { get; }
		public TypeSymbol ValueType { get; }

		public override SymbolType Type => SymbolType.Variable;

		public VariableSymbol(string name, bool isReadOnly, TypeSymbol valueType) {
			Name = name;
			IsReadOnly = isReadOnly;
			ValueType = valueType;
		}
	}
}
