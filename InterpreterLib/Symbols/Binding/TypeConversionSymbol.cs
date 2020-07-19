using InterpreterLib.Symbols.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Symbols.Binding {
	internal class TypeConversionSymbol : Symbol {
		public override SymbolType Type => SymbolType.TypeConversion;

		private static TypeConversionSymbol[] conversions = {
			new TypeConversionSymbol(ValueTypeSymbol.Integer, ValueTypeSymbol.Double),
			new TypeConversionSymbol(ValueTypeSymbol.Byte, ValueTypeSymbol.Double),
			new TypeConversionSymbol(ValueTypeSymbol.Byte, ValueTypeSymbol.Integer),
		};

		public static bool TryFind(TypeSymbol fromType, TypeSymbol toType, out TypeConversionSymbol outputSymbol) {
			outputSymbol = null;

			if (toType.Equals(ValueTypeSymbol.String)) {
				outputSymbol = new TypeConversionSymbol(fromType, toType);
				return true;
			}

			foreach (var symbol in conversions) {
				if (symbol.FromType == fromType && symbol.ToType == toType) {
					outputSymbol = symbol;
					return true;
				}
			}

			return false;
		}

		public override string Name { get; }
		public TypeSymbol FromType { get; }
		public TypeSymbol ToType { get; }

		private TypeConversionSymbol(TypeSymbol fromType, TypeSymbol toType) {
			FromType = fromType;
			ToType = toType;

			Name = $"{FromType} => {ToType}";
		}

		public override bool Equals(object obj) {
			if (!(obj is TypeConversionSymbol symbol)) return false;

			return FromType.Equals(symbol.FromType) && ToType.Equals(symbol.ToType);
		}

		public override int GetHashCode() {
			return HashCode.Combine(FromType, ToType);
		}

		public override string ToString() => Name;
	}
}
