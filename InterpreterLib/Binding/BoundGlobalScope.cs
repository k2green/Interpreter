using InterpreterLib.Binding.Tree;
using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Binding {
	internal sealed class BoundGlobalScope {

		public BoundGlobalScope Previous { get; }
		public IEnumerable<Diagnostic> Diagnostics { get; }
		public IEnumerable<BoundVariable> Variables { get; }
		public BoundNode Root { get; }

		public BoundGlobalScope(BoundGlobalScope previous, IEnumerable<Diagnostic> diagnostics, IEnumerable<BoundVariable> variables, BoundNode root) {
			Previous = previous;
			Diagnostics = diagnostics;
			Variables = variables;
			Root = root;
		}
	}
}
