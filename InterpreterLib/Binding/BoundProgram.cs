using InterpreterLib.Binding.ControlFlow;
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

		internal ControlFlowGraph CreateMainGraph() {
			return ControlFlowGraph.CreateGraph(Statement);
		}

		internal Dictionary<FunctionSymbol, ControlFlowGraph> CreateFunctionGraphs() {
			var outputDictionary = new Dictionary<FunctionSymbol, ControlFlowGraph>();

			foreach(var funcSymbol in FunctionBodies.Keys) {
				outputDictionary.Add(funcSymbol, ControlFlowGraph.CreateGraph(FunctionBodies[funcSymbol]));
			}

			return outputDictionary;
		}
	}
}
