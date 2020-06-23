using System;
using System.Linq;
using InterpreterLib;
using InterpreterLib.Runtime;

namespace Interpreter {
	class Program {
		static void Main(string[] args) {
			string input = null;
			bool running = true;

			Console.CancelKeyPress += delegate {
				Environment.Exit(0);
			};

			BindingEnvironment env = null;

			while (running) {
				Console.Write(" >: ");
				input = Console.ReadLine();

				if (env == null)
					env = new BindingEnvironment(input);
				else
					env = env.ContinueWith(input);

				object output = env.Evaluate();

				if (output != null)
					Console.WriteLine(output);

				foreach (string message in env.Diagnostics)
					Console.WriteLine(message);
			}
		}
	}
}
