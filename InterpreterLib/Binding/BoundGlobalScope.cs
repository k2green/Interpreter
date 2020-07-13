using InterpreterLib.Binding.Tree;
using InterpreterLib.Binding.Tree.Statements;
using InterpreterLib.Symbols.Binding;
using InterpreterLib.Syntax.Tree.Global;
using System.Collections.Generic;

namespace InterpreterLib.Binding {
	internal sealed class BoundGlobalScope {

		public BoundGlobalScope Previous { get; }
		public IEnumerable<VariableSymbol> Variables { get; }
		public IEnumerable<FunctionSymbol> Functions { get; }
		public BoundBlock Root { get; }
		public IDictionary<FunctionSymbol, FunctionDeclarationSyntax> FunctionBodies { get; }

		public BoundGlobalScope(BoundGlobalScope previous, IEnumerable<VariableSymbol> variables, IEnumerable<FunctionSymbol> functions, BoundBlock statement, IDictionary<FunctionSymbol, FunctionDeclarationSyntax> functionBodies) {
			Previous = previous;
			Variables = variables;
			Functions = functions;
			Root = statement;
			FunctionBodies = functionBodies;
		}
	}
}
