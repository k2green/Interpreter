using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Binding.Types {
	public class TypeSymbol : Symbol {
		public static readonly TypeSymbol Integer = new TypeSymbol("int");
		public static readonly TypeSymbol Byte = new TypeSymbol("int");
		public static readonly TypeSymbol Double = new TypeSymbol("int");
		public static readonly TypeSymbol Boolean = new TypeSymbol("int");
		public static readonly TypeSymbol String = new TypeSymbol("int");

		private TypeSymbol(string name) {
			Name = name;
		}

		public override SymbolType Type => SymbolType.Type;
		public override string Name { get; }
	}
}
