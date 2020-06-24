using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using InterpreterLib;
using InterpreterLib.Binding;
using InterpreterLib.Runtime;

namespace Interpreter {
	class Program {
		static void Main(string[] args) {
			string input = null;
			bool running = true;

			Dictionary<BoundVariable, object> variables = new Dictionary<BoundVariable, object>();

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

				if (env == null)
					env = new BindingEnvironment(input, false);
				else
					env = env.ContinueWith(input);

				var output = env.Evaluate(variables);

				if (output.Value != null)
					Console.WriteLine(output.Value);

				foreach (var message in output.Diagnostics)
					Console.WriteLine(message);
			}
		}
	}
}
