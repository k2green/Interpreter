using InterpreterLib.Binding.Tree;
using InterpreterLib.Binding.Types;
using System.Collections.Generic;

namespace InterpreterLib.Binding {
	internal sealed class BoundGlobalScope {

		public BoundGlobalScope Previous { get; }
		public IEnumerable<Diagnostic> Diagnostics { get; }
		public IEnumerable<VariableSymbol> Variables { get; }
		public BoundNode Root { get; }

		public BoundGlobalScope(BoundGlobalScope previous, IEnumerable<Diagnostic> diagnostics, IEnumerable<VariableSymbol> variables, BoundNode root) {
			Previous = previous;
			Diagnostics = diagnostics;
			Variables = variables;
			Root = root;
		}
	}
}
