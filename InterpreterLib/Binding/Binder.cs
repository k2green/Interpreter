using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using InterpreterLib.Binding.Tree;
using InterpreterLib.Binding.Types;
using System.Collections.Generic;

namespace InterpreterLib.Binding {
	internal sealed class Binder : GLangBaseVisitor<BoundNode> {

		private BoundScope scope;
		private DiagnosticContainer diagnostics;

		public Binder(BoundScope parent) {
			scope = new BoundScope(parent);
		}

		public static DiagnosticResult<BoundGlobalScope> BindGlobalScope(BoundGlobalScope previous, IParseTree tree) {
			Binder binder = new Binder(CreateParentScopes(previous));

			BoundNode root = binder.Visit(tree);

			BoundGlobalScope glob = new BoundGlobalScope(previous, binder.diagnostics, binder.scope.GetVariables(), root);
			return new DiagnosticResult<BoundGlobalScope>(binder.diagnostics, glob);
		}

		private static BoundScope CreateParentScopes(BoundGlobalScope previous) {
			if (previous == null)
				return null;

			var stack = new Stack<BoundGlobalScope>();
			while (previous != null) {
				stack.Push(previous);
				previous = previous.Previous;
			}

			BoundScope parent = null;

			while (stack.Count > 0) {
				var scope = new BoundScope(parent);
				previous = stack.Pop();

				foreach (VariableSymbol variable in previous.Variables)
					scope.TryDefine(variable);

				parent = scope;
			}

			return parent;
		}

		private BoundError Error(Diagnostic diagnostic) {
			return new BoundError
		}

		public override BoundNode VisitLiteral([NotNull] GLangParser.LiteralContext context) {
			return base.VisitLiteral(context);
		}
	}
}
