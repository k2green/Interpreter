using InterpreterLib.Runtime;
using InterpreterLib.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime;
using Console = Colorful.Console;
using System.Drawing;
using InterpreterLib.Syntax.Tree;
using InterpreterLib;
using InterpreterLib.Output;
using InterpreterLib.Symbols.Binding;

namespace Interpreter {
	public sealed class LanguageRepl : Repl {

		private static readonly Color DEFAULT_COLOR = Color.White;

		private SyntaxTree tree;

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
			tree = new SyntaxTree(input);

			if (environment == null)
				environment = BindingEnvironment.CreateEnvironment(tree, false);
			else
				environment = environment.ContinueWith(tree);

			if (tree.Diagnostics.Any()) {
				PrintDiagnostic(tree.Diagnostics);
			} else if (showTree) {
				var treeOutput = new SyntaxTreeOutput(tree);
				treeOutput.Output(Console.Write, Console.WriteLine);
			}

			if (environment != null) {
				if (environment != null && showProgram)
					environment.PrintProgram(Console.Write, Console.WriteLine);

				Console.ForegroundColor = DEFAULT_COLOR;
				Evaluate(environment, variables);
			}
		}

		private void PrintDiagnostic(Diagnostic diagnostic) {
			Console.WriteLine($"({diagnostic.Line}: {diagnostic.Column}) {diagnostic.Message}", Color.Red);
			Console.Write("\t");
			Console.Write(tree.Source.GetSubstring(diagnostic.PreviousText), DEFAULT_COLOR);
			Console.Write(" ", DEFAULT_COLOR);
			Console.Write(tree.Source.GetSubstring(diagnostic.OffendingText), Color.Red);
			Console.Write(" ", DEFAULT_COLOR);
			Console.Write(tree.Source.GetSubstring(diagnostic.NextText), DEFAULT_COLOR);
			Console.WriteLine();
		}

		private void PrintDiagnostic(IEnumerable<Diagnostic> diagnostics) {
			foreach (var diagnostic in diagnostics)
				PrintDiagnostic(diagnostic);
		}

		public void Evaluate(BindingEnvironment env, Dictionary<VariableSymbol, object> variables) {
			var res = env.Evaluate(variables);

			if (res.Diagnostics.Any()) {
				PrintDiagnostic(res.Diagnostics);
			} else if (evaluate && res.Value != null) {
				Console.WriteLine(res.Value, DEFAULT_COLOR);
			}
		}

		protected override void RenderLine(string line) {
			if (line.StartsWith("#")) {
				Console.WriteLine(line);
			} else {
				tree = new SyntaxTree(line, false);
				var tokens = tree.Tokens;
				var vocab = tree.Vocabulary;

				foreach (var token in tokens) {
					if (!vocab.GetDisplayName(token.Type).ToLower().Equals("eof"))
						RenderToken(token, vocab);
				}
			}
		}

		private void RenderToken(IToken token, IVocabulary vocab) {
			switch (vocab.GetDisplayName(token.Type)) {
				case "INTEGER":
				case "DOUBLE":
				case "BOOLEAN":
					Console.ForegroundColor = Color.Aquamarine;
					break;

				case "STRING":
					Console.ForegroundColor = Color.FromArgb(214, 157, 133);
					break;

				case "FUNCTION":
				case "DECL_VARIABLE":
					Console.ForegroundColor = Color.Violet;
					break;

				case "IF":
				case "ELSE":
				case "WHILE":
				case "FOR":
				case "BREAK":
				case "CONTINUE":
					Console.ForegroundColor = Color.MediumVioletRed;
					break;

				case "TYPE_NAME":
					Console.ForegroundColor = Color.Orange;
					break;

				case "IDENTIFIER":
					Console.ForegroundColor = Color.Yellow;
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
				case "graphpath":
					Console.WriteLine(BindingEnvironment.GraphPath);
					return true;
				default:
					return false;
			}
		}
	}
}
