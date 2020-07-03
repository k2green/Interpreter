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
			bool showTree = false;

			Dictionary<VariableSymbol, object> variables = new Dictionary<VariableSymbol, object>();

			Console.CancelKeyPress += delegate {
				Environment.Exit(0);
			};

			BindingEnvironment env = null;

			while (running) {
				Console.ForegroundColor = ConsoleColor.White;
				Console.Write(">: ");
				input = Console.ReadLine();
				/*StringBuilder builder = new StringBuilder();
				while (!(input = Console.ReadLine()).Equals("#quit")) {
					builder.Append(input);
				}*/

				if (input.Equals("#clear")) {
					Console.Clear();
				} else if (input.Equals("#clearvariables")) {
					variables = new Dictionary<VariableSymbol, object>();
					env = null;
				} else if (input.Equals("#showTree")) {
					showTree = !showTree;
					Console.WriteLine($"Showing tree: {showTree}");
				} else {
					if (env == null)
						env = new BindingEnvironment(input, false);
					else
						env = env.ContinueWith(input);

					Evaluate(env, variables);

					if (showTree)
						Text(env);
				}
			}
		}

		public static void Evaluate(BindingEnvironment env, Dictionary<VariableSymbol, object> variables) {
			var res = env.Evaluate(variables);

			if (res.Diagnostics.Any())
				foreach (var diagnostic in res.Diagnostics)
					Console.WriteLine(diagnostic);
			else
				Console.WriteLine(res.Value);
		}

		public static void Text(BindingEnvironment env) {
			env.PrintText();
		}
	}
}
