using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Symbols.Types {
	internal sealed class ArraySymbol : TypeSymbol {
		public override SymbolType Type => SymbolType.Array;

		public override string Name => $"Array({ValueType})";

		public TypeSymbol ValueType { get; }

		public ArraySymbol(TypeSymbol type) {
			ValueType = type;
		}

		public override bool Equals(object obj) {
			if (!(obj is ArraySymbol symbol)) return false;

			return ValueType.Equals(symbol.ValueType);
		}

		public override int GetHashCode() {
			return ValueType.GetHashCode();
		}
	}
}
