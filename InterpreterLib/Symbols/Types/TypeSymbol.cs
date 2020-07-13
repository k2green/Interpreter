using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Symbols.Types {
	public abstract class TypeSymbol : Symbol {

		public static TypeSymbol FromString(string input) {
			if (ValueTypeSymbol.TryGetFromString(input, out var valueTypeSymbol))
				return valueTypeSymbol;

			return null;
		}
	}
}
