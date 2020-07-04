using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Binding.Types {
	public enum SymbolType {
		Variable, Type,
		Function,
		Parameter,
		Label,
		TypeConversion
	}
}
