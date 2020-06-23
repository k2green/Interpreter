using InterpreterLib.Binding.Tree;
using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Binding {
	internal sealed class BoundGlobalScope {

		public BoundGlobalScope Previous { get; }
		public IEnumerable<string> Diagnostics { get; }
		public IEnumerable<BoundVariable> Variables { get; }
		public BoundExpression Root { get; }

		public BoundGlobalScope(BoundGlobalScope previous, IEnumerable<string> diagnostics, IEnumerable<BoundVariable> variables, BoundExpression root) {
			Previous = previous;
			Diagnostics = diagnostics;
			Variables = variables;
			Root = root;
		}
	}
}
