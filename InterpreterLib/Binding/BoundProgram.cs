using InterpreterLib.Binding.Tree.Statements;
using InterpreterLib.Symbols.Binding;
using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Binding {
	public class BoundProgram {

		internal Dictionary<FunctionSymbol, BoundBlock> FunctionBodies { get; }
		internal BoundBlock Statement { get; }

		internal BoundProgram(Dictionary<FunctionSymbol, BoundBlock> functionBodies, BoundBlock statement) {
			FunctionBodies = functionBodies;
			Statement = statement;
		}
	}
}
