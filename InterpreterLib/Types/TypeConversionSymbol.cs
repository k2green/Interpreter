using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Types {
	internal class TypeConversionSymbol : Symbol {
		public override SymbolType Type => SymbolType.TypeConversion;

		private static TypeConversionSymbol[] conversions = {
			new TypeConversionSymbol(TypeSymbol.Integer, TypeSymbol.Double),
			new TypeConversionSymbol(TypeSymbol.Byte, TypeSymbol.Double),
			new TypeConversionSymbol(TypeSymbol.Byte, TypeSymbol.Integer),

			new TypeConversionSymbol(TypeSymbol.Integer, TypeSymbol.String),
			new TypeConversionSymbol(TypeSymbol.Double, TypeSymbol.String),
			new TypeConversionSymbol(TypeSymbol.Byte, TypeSymbol.String),
			new TypeConversionSymbol(TypeSymbol.Boolean, TypeSymbol.String),
			new TypeConversionSymbol(TypeSymbol.Character, TypeSymbol.String)
		};

		public static bool TryFind(TypeSymbol fromType, TypeSymbol toType, out TypeConversionSymbol outputSymbol) {
			outputSymbol = null;

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

			return FromType == symbol.FromType && ToType == symbol.ToType;
		}

		public override int GetHashCode() {
			return HashCode.Combine(FromType, ToType);
		}

		public override string ToString() => Name;
	}
}
