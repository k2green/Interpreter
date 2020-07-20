using InterpreterLib.Symbols.Binding;
using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Evaluation.Objects {
	internal class FunctionPointerObject : RuntimeObject {

		public FunctionSymbol Function { get; }

		public FunctionPointerObject(FunctionSymbol function) {
			Function = function;
		}
	}
}
