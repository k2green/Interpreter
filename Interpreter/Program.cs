using System;
using System.Linq;
using System.Collections.Generic;
using InterpreterLib.Types;
using InterpreterLib.Runtime;
using InterpreterLib.Syntax;

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
			var res = env.Evaluate(variables);

			if (res.Diagnostics.Any())
				foreach (var diagnostic in res.Diagnostics)
					Console.WriteLine(diagnostic);
			else
				Console.WriteLine(res.Value);
		}
	}
}
