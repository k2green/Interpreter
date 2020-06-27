using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Binding.Types {
	public class TypeSymbol : Symbol {
		public static readonly TypeSymbol Integer = new TypeSymbol("int");
		public static readonly TypeSymbol Byte = new TypeSymbol("byte");
		public static readonly TypeSymbol Double = new TypeSymbol("double");
		public static readonly TypeSymbol Boolean = new TypeSymbol("bool");
		public static readonly TypeSymbol String = new TypeSymbol("string");

		private TypeSymbol(string name) {
			Name = name;
		}

		public override SymbolType Type => SymbolType.Type;
		public override string Name { get; }

		public override bool Equals(object obj) {
			if (!(obj is TypeSymbol)) return false;

			TypeSymbol symbol = (TypeSymbol)obj;
			return this == symbol;
		}

		public override int GetHashCode() {
			return Name.GetHashCode();
		}
	}
}
