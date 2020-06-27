using InterpreterLib.Binding.Tree;
using InterpreterLib.Binding.Types;
using System.Collections.Generic;

namespace InterpreterLib.Binding {
	internal sealed class BoundGlobalScope {

		public BoundGlobalScope Previous { get; }
		public IEnumerable<VariableSymbol> Variables { get; }
		public BoundNode Root { get; }

		public BoundGlobalScope(BoundGlobalScope previous, IEnumerable<VariableSymbol> variables, BoundNode root) {
			Previous = previous;
			Variables = variables;
			Root = root;
		}
	}
}
