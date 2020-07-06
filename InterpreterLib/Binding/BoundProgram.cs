using InterpreterLib.Binding.Tree.Statements;
using InterpreterLib.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Binding {
	internal class BoundProgram {

		public Dictionary<FunctionSymbol, BoundBlock> FunctionBodies { get; }
		public BoundBlock Statement { get; }

		public BoundProgram(Dictionary<FunctionSymbol, BoundBlock> functionBodies, BoundBlock statement) {
			FunctionBodies = functionBodies;
			Statement = statement;
		}
	}
}
