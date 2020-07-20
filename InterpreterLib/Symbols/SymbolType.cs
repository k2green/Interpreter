using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Symbols {
	public enum SymbolType {
		Variable, Type,
		Function,
		Parameter,
		Label,
		TypeConversion,
		Tuple,
		Array,
		FunctionPointer,
		FunctionType
	}
}
