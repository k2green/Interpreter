using System;
using System.Linq;
using System.Collections.Generic;
using InterpreterLib.Binding.Types;
using InterpreterLib.Runtime;
using InterpreterLib.Syntax;

namespace Interpreter {
	class Program {
		static void Main(string[] args) {
			string input = null;
			bool running = true;

			Dictionary<VariableSymbol, object> variables = new Dictionary<VariableSymbol, object>();

			Console.CancelKeyPress += delegate {
				Environment.Exit(0);
			};

			BindingEnvironment env = null;

			while (running) {
				input = Console.ReadLine();
				/*StringBuilder builder = new StringBuilder();
				while (!(input = Console.ReadLine()).Equals("#quit")) {
					builder.Append(input);
				}*/

				if (input.ToLower().Equals("clear")) {
					Console.Clear();
				} else {
					if (env == null)
						env = new BindingEnvironment(input, false);
					else
						env = env.ContinueWith(input);

					var res = env.ToText();

					foreach (var diagnostic in res.Diagnostics)
						Console.WriteLine(diagnostic);

					foreach (var line in res.Value)
						Console.WriteLine(line);
				}
			}
		}
	}
}
