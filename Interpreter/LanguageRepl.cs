using InterpreterLib.Binding.Types;
using InterpreterLib.Runtime;
using InterpreterLib.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Interpreter {
	public sealed class LanguageRepl : Repl {

		public LanguageRepl() : base() { }

		public LanguageRepl(bool showTree, bool showProgram, bool multiLine, bool evaluate) : base(showTree, showProgram, multiLine, evaluate) { }

		protected override void EvaluateInput(string input) {
			var parser = new RuntimeParser(input);

			if (environment == null)
				environment = BindingEnvironment.CreateEnvironment(parser, false);
			else
				environment = environment.ContinueWith(parser);

			if (parser.Diagnostics.Any()) {
				Console.ForegroundColor = ConsoleColor.Red;
				foreach (var diagnostic in parser.Diagnostics)
					Console.WriteLine(diagnostic);

				Console.ForegroundColor = ConsoleColor.White;
			} else if (showTree) {
				SyntaxTreeWriter.Write(parser, Console.Out);
			}

			if (environment != null) {
				if (environment != null && showProgram)
					environment.PrintText();

				Console.ForegroundColor = ConsoleColor.White;
				Evaluate(environment, variables);
			}
		}

		protected override bool IsCompleteSubmission(string input) {
			if (!base.IsCompleteSubmission(input))
				return false;

			parser = new RuntimeParser(input);

			if (parser.Diagnostics.Any())
				return false;

			return true;
		}

		public void Evaluate(BindingEnvironment env, Dictionary<VariableSymbol, object> variables) {
			var res = env.Evaluate(variables);

			if (res.Diagnostics.Any()) {
				Console.ForegroundColor = ConsoleColor.Red;
				foreach (var diagnostic in res.Diagnostics)
					Console.WriteLine(diagnostic);

				Console.ForegroundColor = ConsoleColor.White;
			} else if (evaluate && res.Value != null) {
				Console.WriteLine(res.Value);
			}
		}
	}
}
