using InterpreterLib.Symbols.Binding;
using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Evaluation.Objects {
	internal interface ScopedObject {
		Dictionary<VariableSymbol, object> Variables { get; }
	}
}
