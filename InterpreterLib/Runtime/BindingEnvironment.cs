using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using InterpreterLib.Binding;
using InterpreterLib.Binding.Tree;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace InterpreterLib.Runtime {
	public sealed class BindingEnvironment {
		private Dictionary<BoundVariable, object> variables;
		private DiagnosticContainer diagnostics;
		private BoundGlobalScope globalScope;
		private BindingEnvironment previous;
		private IParseTree SyntaxRoot;

		public IEnumerable<string> StringDiagnostics => diagnostics.Messages;

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

		public BindingEnvironment(string input) : this(input, null) {
			variables = new Dictionary<BoundVariable, object>();
		}

		public BindingEnvironment ContinueWith(string input) {
			if (GlobalScope.Root != null)
				return new BindingEnvironment(input, this);

			return new BindingEnvironment(input, previous);
		}

		private BindingEnvironment(string input, BindingEnvironment previous) {
			this.previous = previous;
			diagnostics = new DiagnosticContainer();
			AntlrInputStream stream = new AntlrInputStream(input);

			GLangLexer lexer = new GLangLexer(stream);
			lexer.RemoveErrorListeners();

			CommonTokenStream tokens = new CommonTokenStream(lexer);
			GLangParser parser = new GLangParser(tokens);
			parser.RemoveErrorListeners();

			if (previous != null) {
				diagnostics.AddDiagnostics(previous.diagnostics);
				variables = previous.variables;
			}

			SyntaxRoot = parser.statement();
		}

		public object Evaluate() {
			if (GlobalScope.Root == null)
				return null;

			foreach (BoundVariable variable in GlobalScope.Variables)
				if(variable != null)
				variables.Add(variable, null);

			Evaluator evaluator = new Evaluator(GlobalScope.Root, variables);

			var evalResult = evaluator.Evaluate();
			diagnostics.AddDiagnostics(evalResult.Diagnostics);
			return evalResult.Value;
		}
	}
}
