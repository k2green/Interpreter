using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using InterpreterLib.Binding;
using InterpreterLib.Binding.Lowering;
using InterpreterLib.Binding.Tree;
using InterpreterLib.Binding.Types;
using InterpreterLib.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace InterpreterLib.Runtime {
	public sealed class BindingEnvironment {
		private DiagnosticContainer diagnostics;
		private BoundGlobalScope globalScope;
		private readonly bool chainDiagnostics;
		private BindingEnvironment previous;
		private IParseTree SyntaxRoot;

		public IEnumerable<Diagnostic> Diagnostics => diagnostics;

		internal BoundGlobalScope GlobalScope {
			get {
				if (globalScope == null) {
					var prevScope = previous == null ? null : previous.GlobalScope;
					//var binderResult = Binder.BindGlobalScope(prevScope, SyntaxRoot);
					//Interlocked.CompareExchange<BoundGlobalScope>(ref globalScope, binderResult.Value, null);
					//diagnostics.AddDiagnostics(binderResult.Diagnostics);
				}

				return globalScope;
			}
		}

		public BindingEnvironment(string input, bool chainDiagnostics) : this(input, chainDiagnostics, null) {
		}

		public BindingEnvironment ContinueWith(string input) {
			return new BindingEnvironment(input, chainDiagnostics, this);
		}

		private BindingEnvironment(string input, bool chainDiagnostics, BindingEnvironment previous) {
			this.chainDiagnostics = chainDiagnostics;
			this.previous = previous;
			diagnostics = new DiagnosticContainer();
			AntlrInputStream stream = new AntlrInputStream(input);

			GLangLexer lexer = new GLangLexer(stream);
			lexer.RemoveErrorListeners();

			CommonTokenStream tokens = new CommonTokenStream(lexer);
			GLangParser parser = new GLangParser(tokens);
			parser.RemoveErrorListeners();

			if (previous != null) {
				if (chainDiagnostics) {
					diagnostics.AddDiagnostics(previous.diagnostics);
				}
			}

			SyntaxRoot = parser.statement();
		}

		public DiagnosticResult<object> Evaluate(Dictionary<VariableSymbol, object> variables) {
			if (GlobalScope.Root == null)
				return new DiagnosticResult<object>(diagnostics, null);

			Evaluator evaluator = new Evaluator(GlobalScope.Root, variables);

			var evalResult = evaluator.Evaluate();
			diagnostics.AddDiagnostics(evalResult.Diagnostics);
			return new DiagnosticResult<object>(diagnostics, evalResult.Value);
		}

		public IEnumerable<string> ToText() {
			BoundTreeDisplayVisitor display = new BoundTreeDisplayVisitor();
			var lines = display.GetText(GlobalScope.Root);
			lines = lines.Concat(new string[] { "", "", "" });

			if (!diagnostics.Any() && GlobalScope.Root is BoundStatement) {
				display = new BoundTreeDisplayVisitor();
				lines = lines.Concat(display.GetText(Lowerer.Lower((BoundStatement)GlobalScope.Root)));
			}

			return lines;
		}
	}
}
