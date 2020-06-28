using System;
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

				SyntaxTreeWriter.ParseAndWriteTree(Console.Out, input);
			}
		}
	}
}
