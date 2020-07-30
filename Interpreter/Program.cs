using System;
using System.Linq;
using System.Collections.Generic;
using InterpreterLib.Runtime;
using InterpreterLib.Syntax;
using InterpreterLib.Symbols.Binding;

namespace Interpreter {
	class Program {
		static void Main(string[] args) {
			Console.CancelKeyPress += delegate {
				Environment.Exit(0);
			};

			var repl = new LanguageRepl(false, true, true, true);
			repl.Run();
		}

		public static void Evaluate(BindingEnvironment env, Dictionary<VariableSymbol, object> variables) {
			var res = env.CurrentInterpretation.Evaluate(variables);

			if (res.Diagnostics.Any())
				foreach (var diagnostic in res.Diagnostics)
					Console.WriteLine(diagnostic);
			else
				Console.WriteLine(res.Value);
		}
	}
}
