using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace InterpreterLib.Types {
	public class TypeSymbol : Symbol {
		public static readonly TypeSymbol Integer = new TypeSymbol("int");
		public static readonly TypeSymbol Byte = new TypeSymbol("byte");
		public static readonly TypeSymbol Double = new TypeSymbol("double");
		public static readonly TypeSymbol Boolean = new TypeSymbol("bool");
		public static readonly TypeSymbol String = new TypeSymbol("string");
		public static readonly TypeSymbol Void = new TypeSymbol("void");

		public static IEnumerable<TypeSymbol> GetAll() {
			return typeof(TypeSymbol).GetFields(BindingFlags.Public | BindingFlags.Static)
				.Where(f => f.FieldType == typeof(TypeSymbol))
				.Select(f => (TypeSymbol)f.GetValue(null));
		}

		public static TypeSymbol FromString(string input) {
			foreach (var symbol in GetAll())
				if (symbol.Name.Equals(input))
					return symbol;

			return null;
		}

		public static bool GetType(string typeStr, out TypeSymbol symbol) {
			foreach (var type in GetAll()) {
				if (type.Name.Equals(typeStr)) {
					symbol = type;
					return true;
				}
			}

			symbol = null;
			return false;
		}

		private TypeSymbol(string name) {
			Name = name;
		}

		public override SymbolType Type => SymbolType.Type;
		public override string Name { get; }

		public override string ToString() => Name;

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
