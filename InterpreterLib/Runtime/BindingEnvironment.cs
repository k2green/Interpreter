using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using InterpreterLib.Binding;
using InterpreterLib.Binding.Lowering;
using InterpreterLib.Binding.Tree;
using InterpreterLib.Binding.Tree.Statements;
using InterpreterLib.Binding.Types;
using InterpreterLib.Diagnostics;
using InterpreterLib.Syntax;
using InterpreterLib.Syntax.Tree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace InterpreterLib.Runtime {
	public sealed class BindingEnvironment {
		private DiagnosticContainer diagnostics;
		private readonly bool chainDiagnostics;
		private BindingEnvironment previous;
		private SyntaxNode SyntaxRoot;

		public IEnumerable<Diagnostic> Diagnostics => diagnostics;

		public static BindingEnvironment CreateEnvironment(RuntimeParser input, bool chainDiagnostics) {
			if (input.Diagnostics.Any())
				return null;

			return new BindingEnvironment(input.Node, chainDiagnostics, null);
		}

		public BindingEnvironment ContinueWith(RuntimeParser input) {
			return new BindingEnvironment(input.Node, chainDiagnostics, this);
		}


		private BoundGlobalScope globalScope;
		internal BoundGlobalScope GlobalScope {
			get {
				if (globalScope == null) {
					var prevScope = previous == null ? null : previous.GlobalScope;
					var binderResult = Binder.BindGlobalScope(prevScope, SyntaxRoot);
					Interlocked.CompareExchange<BoundGlobalScope>(ref globalScope, binderResult.Value, null);
					diagnostics.AddDiagnostics(binderResult.Diagnostics);
				}

				return globalScope;
			}
		}

		private BindingEnvironment(SyntaxNode input, bool chainDiagnostics, BindingEnvironment previous) {
			this.chainDiagnostics = chainDiagnostics;
			this.previous = previous;
			diagnostics = new DiagnosticContainer();

			if (previous != null && chainDiagnostics) {
				diagnostics.AddDiagnostics(previous.diagnostics);
			}

			SyntaxRoot = input;
		}

		public DiagnosticResult<object> Evaluate(Dictionary<VariableSymbol, object> variables) {
			if (GlobalScope == null || GlobalScope.Root == null || diagnostics.Any())
				return new DiagnosticResult<object>(diagnostics, null);

			Evaluator evaluator = new Evaluator(GlobalScope.Root, variables);

			var evalResult = evaluator.Evaluate();
			diagnostics.AddDiagnostics(evalResult.Diagnostics);
			return new DiagnosticResult<object>(diagnostics, evalResult.Value);
		}

		public void PrintText() {
			if (GlobalScope != null) {
				BoundTreeDisplayVisitor display = new BoundTreeDisplayVisitor();

				if (!diagnostics.Any() && GlobalScope.Root != null) {
					display.GetText(GlobalScope.Root);
				}
			}
		}
	}
}
