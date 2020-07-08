using InterpreterLib.Types;
using InterpreterLib.Runtime;
using InterpreterLib.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime;
using Console = Colorful.Console;
using System.Drawing;

namespace Interpreter {
	public sealed class LanguageRepl : Repl {

		private static readonly Color DEFAULT_COLOR = Color.White;

		private bool showTree;
		private bool showProgram;
		private bool evaluate;

		public LanguageRepl() : this(false, false, false, true) { }

		public LanguageRepl(bool shouldShowTree, bool shouldShowProgram, bool isMultiLine, bool shouldEvaluate) : base(isMultiLine) {
			showTree = shouldShowTree;
			showProgram = shouldShowProgram;
			evaluate = shouldEvaluate;
		}

		protected override void EvaluateInput(string input) {
			var parser = new RuntimeParser(input);

			if (environment == null)
				environment = BindingEnvironment.CreateEnvironment(parser, false);
			else
				environment = environment.ContinueWith(parser);

			if (parser.Diagnostics.Any()) {
				foreach (var diagnostic in parser.Diagnostics)
					Console.WriteLine(diagnostic, Color.Red);
			} else if (showTree) {
				SyntaxTreeWriter.Write(parser, Console.Out);
			}

			if (environment != null) {
				if (environment != null && showProgram)
					environment.PrintText();

				Console.ForegroundColor = DEFAULT_COLOR;
				Evaluate(environment, variables);
			}
		}

		protected override 

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
				foreach (var diagnostic in res.Diagnostics)
					Console.WriteLine(diagnostic, Color.Red);
			} else if (evaluate && res.Value != null) {
				Console.WriteLine(res.Value, DEFAULT_COLOR);
			}
		}

		protected override void RenderLine(string line) {
			if(line.StartsWith("#")) {
				Console.WriteLine(line);
			} else {
				var res = RuntimeParser.GetTokens(line);
				var tokens = res.Item1;
				var vocab = res.Item2;
				tokens.Fill();

				foreach (var token in tokens.GetTokens()) {
					if (!vocab.GetDisplayName(token.Type).ToLower().Equals("eof"))
						RenderToken(token, vocab);
				}
				
				Console.WriteLine();
			}
		}

		private void RenderToken(IToken token, IVocabulary vocab) {
			switch(vocab.GetDisplayName(token.Type)) {
				case "INTEGER":
				case "DOUBLE":
				case "BOOLEAN":
					Console.ForegroundColor = Color.Magenta;
					break;

				case "STRING":
					Console.ForegroundColor = Color.FromArgb(214, 157, 133);
					break;

				case "DECL_VARIABLE":
					Console.ForegroundColor = Color.DarkCyan;
					break;

				case "FUNCTION":
				case "IF":
				case "ELSE":
				case "WHILE":
				case "FOR":
					Console.ForegroundColor = Color.SlateBlue;
					break;

				case "TYPE_NAME":
					Console.ForegroundColor = Color.Orange;
					break;

				case "IDENTIFIER":
					Console.ForegroundColor = Color.Aquamarine;
					break;

				case "UNKNOWN":
					Console.ForegroundColor = Color.Red;
					break;

				default:
					Console.ForegroundColor = DEFAULT_COLOR;
					break;
			}

			Console.Write(token.Text);
		}

		protected override bool EvaluateCommand(string command) {
			string outputString;

			switch (command.ToLower()) {
				case "clear":
					Console.Clear();
					return true;
				case "reset":
					variables = new Dictionary<VariableSymbol, object>();
					environment = null;
					return true;
				case "showtree":
					showTree = !showTree;
					outputString = $"{(showTree ? "S" : "Not s")}howing syntax tree";
					Console.WriteLine(outputString, DEFAULT_COLOR);
					return true;
				case "multiline":
					multiLine = !multiLine;
					outputString = $"Mode: {(multiLine ? "Multiple lines\nPress Ctrl+Enter to evaluate" : "SingleLine")}";
					Console.WriteLine(outputString, DEFAULT_COLOR);
					return true;
				case "showprogram":
					showProgram = !showProgram;
					outputString = $"{(showProgram ? "S" : "Not s")}howing bound tree";
					Console.WriteLine(outputString, DEFAULT_COLOR);
					return true;
				case "evaluate":
					evaluate = !evaluate;
					outputString = $"{(evaluate ? "E" : "Not e")}valuating bound tree";
					Console.WriteLine(outputString, DEFAULT_COLOR);
					return true;
				case "exit":
					running = false;
					return true;
				default:
					return false;
			}
		}
	}
}
