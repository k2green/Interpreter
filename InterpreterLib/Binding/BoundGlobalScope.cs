using InterpreterLib.Binding.Tree;
using InterpreterLib.Binding.Tree.Statements;
using InterpreterLib.Binding.Types;
using System.Collections.Generic;

namespace InterpreterLib.Binding {
	internal sealed class BoundGlobalScope {

		public BoundGlobalScope Previous { get; }
		public IEnumerable<VariableSymbol> Variables { get; }
		public BoundStatement[] Root { get; }
		public BoundStatement UnLoweredRoot { get; }

		public BoundStatement FirstStatement {
			get {
				if (Root == null || Root.Length < 1)
					return null;

				return Root[0];
			}
		}

		public BoundGlobalScope(BoundGlobalScope previous, IEnumerable<VariableSymbol> variables, BoundStatement unLoweredRoot, BoundStatement[] statements) {
			Previous = previous;
			Variables = variables;
			UnLoweredRoot = unLoweredRoot;
			Root = statements;
		}
	}
}
