using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace InterpreterLib.Symbols.Types {
	public class ValueTypeSymbol : TypeSymbol {
		public static readonly ValueTypeSymbol Integer = new ValueTypeSymbol("int");
		public static readonly ValueTypeSymbol Byte = new ValueTypeSymbol("byte");
		public static readonly ValueTypeSymbol Double = new ValueTypeSymbol("double");
		public static readonly ValueTypeSymbol Boolean = new ValueTypeSymbol("bool");
		public static readonly ValueTypeSymbol String = new ValueTypeSymbol("string");
		public static readonly ValueTypeSymbol Character = new ValueTypeSymbol("char");
		public static readonly ValueTypeSymbol Void = new ValueTypeSymbol("void");

		public static IEnumerable<ValueTypeSymbol> GetAll() {
			return typeof(ValueTypeSymbol).GetFields(BindingFlags.Public | BindingFlags.Static)
				.Where(f => f.FieldType == typeof(ValueTypeSymbol))
				.Select(f => (ValueTypeSymbol)f.GetValue(null));
		}

		public static bool TryGetFromString(string input, out ValueTypeSymbol outSymbol) {

			foreach (var symbol in GetAll()) {
				if (symbol.Name.Equals(input)) {
					outSymbol = symbol;
					return true;
				}
			}

			outSymbol = null;
			return false;
		}

		public static bool GetType(string typeStr, out ValueTypeSymbol symbol) {
			foreach (var type in GetAll()) {
				if (type.Name.Equals(typeStr)) {
					symbol = type;
					return true;
				}
			}

			symbol = null;
			return false;
		}

		private ValueTypeSymbol(string name) {
			Name = name;
		}

		public override SymbolType Type => SymbolType.Type;
		public override string Name { get; }

		public override string ToString() => Name;

		public override bool Equals(object obj) {
			if (!(obj is ValueTypeSymbol symbol)) return false;

			return Name.Equals(symbol.Name);
		}

		public override int GetHashCode() {
			return Name.GetHashCode();
		}
	}
}
