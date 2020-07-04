using InterpreterLib.Binding.Types;
using InterpreterLib.Runtime;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Interpreter {
	public abstract class Repl {

		protected RuntimeParser parser;
		protected bool done;
		protected bool running;

		protected bool showTree;
		protected bool showProgram;
		protected bool multiLine;
		protected bool evaluate;

		protected Dictionary<VariableSymbol, object> variables;
		protected BindingEnvironment environment;

		public Repl() : this(false, false, false, true) { }

		public Repl(bool showTree, bool showProgram, bool multiLine, bool evaluate) {
			this.showTree = showTree;
			this.showProgram = showProgram;
			this.multiLine = multiLine;
			this.evaluate = evaluate;
		}

		public void Run() {
			variables = new Dictionary<VariableSymbol, object>();
			running = true;

			while (running) {
				string input = EditSubmission();

				if (string.IsNullOrEmpty(input))
					return;

				if (IsCommand(input)) {
					if (!EvaluateCommand(input.Substring(1, input.Length - 1)))
						Console.WriteLine($"Invalid command {input}");
				} else {
					EvaluateInput(input);
				}
			}
		}

		private string EditSubmission() {
			var observable = new ObservableCollection<string>() { "" };
			var view = new SubmissionView(observable);
			done = false;

			while (!done) {
				var key = Console.ReadKey(true);
				HandleKey(key, observable, view);
			}

			Console.WriteLine();

			return string.Join(Environment.NewLine, observable);
		}

		private void HandleKey(ConsoleKeyInfo key, ObservableCollection<string> observable, SubmissionView view) {
			if (key.Modifiers == default) {
				switch (key.Key) {
					case ConsoleKey.LeftArrow:
						HandleLeftArrow(observable, view);
						return;
					case ConsoleKey.RightArrow:
						HandleRightArrow(observable, view);
						return;
					case ConsoleKey.UpArrow:
						HandleUpArrow(observable, view);
						return;
					case ConsoleKey.DownArrow:
						HandleDownArrow(observable, view);
						return;
					case ConsoleKey.Enter:
						HandleEnter(observable, view);
						break;
					case ConsoleKey.Backspace:
						HandleBackspace(observable, view);
						break;
				}
			} else if (key.Modifiers.HasFlag(ConsoleModifiers.Control)) {
				switch (key.Key) {
					case ConsoleKey.Enter:
						HandleCtrlEnter(observable, view);
						break;
				}
			}

			if (key.KeyChar >= ' ') {
				HandleTyping(observable, view, key.KeyChar.ToString());
			}
		}

		private void HandleCtrlEnter(ObservableCollection<string> observable, SubmissionView view) {
			done = true;
		}

		private void HandleBackspace(ObservableCollection<string> observable, SubmissionView view) {
			int line = view.TextLine;
			int column = view.CursorCharacter;

			if (column > 0) {
				string text = observable[line].Remove(column - 1, 1);
				observable[line] = text;
				view.CursorCharacter--;
			} else {
				if(string.IsNullOrEmpty(observable[line]) && observable.Count > 1) {
					observable.RemoveAt(line);
					view.CursorLine--;
					view.CursorCharacter = observable[view.TextLine].Length;
				}
			}
		}


		private void HandleTyping(ObservableCollection<string> observable, SubmissionView view, string text) {
			int line = view.TextLine;
			int start = view.CursorCharacter;

			observable[line] = observable[line].Insert(start, text);
			view.CursorCharacter += text.Length;
		}

		private void HandleEnter(ObservableCollection<string> observable, SubmissionView view) {
			if (!multiLine || (observable.Count == 1 && IsCommand(observable[0]))) {
				HandleCtrlEnter(observable, view);
			} else {
				int line = view.TextLine;
				var lineText = observable[line];

				if (view.CursorCharacter == lineText.Length) {
					observable.Insert(line + 1, string.Empty);
				} else {
					string start = lineText.Substring(0, view.CursorCharacter);
					string end = lineText.Substring(view.CursorCharacter, lineText.Length - view.CursorCharacter);
					observable[line] = start;
					observable.Insert(line + 1, end);
				}

				view.CursorCharacter = 0;
				view.CursorLine++;
			}
		}

		private void HandleDownArrow(ObservableCollection<string> observable, SubmissionView view) {
			if (view.TextLine < observable.Count - 1)
				view.CursorLine++;

			var line = observable[view.TextLine];
			if (view.CursorCharacter > line.Length)
				view.CursorCharacter = line.Length;
		}

		private void HandleUpArrow(ObservableCollection<string> observable, SubmissionView view) {
			if (view.TextLine > 0)
				view.CursorLine--;

			var line = observable[view.TextLine];
			if (view.CursorCharacter >= line.Length)
				view.CursorCharacter = line.Length;
		}

		private void HandleRightArrow(ObservableCollection<string> observable, SubmissionView view) {
			var line = observable[view.TextLine];
			if (view.CursorCharacter < line.Length) {
				view.CursorCharacter++;
			} else if (view.TextLine < observable.Count - 1) {
				view.CursorLine++;
				view.CursorCharacter = 0;
			}
		}

		private void HandleLeftArrow(ObservableCollection<string> observable, SubmissionView view) {
			if (view.CursorCharacter > 0) {
				view.CursorCharacter--;
			} else if(view.TextLine > 0) {
				view.CursorLine--;
				view.CursorCharacter = observable[view.TextLine].Length;
			}
		}

		protected abstract void EvaluateInput(string input);

		protected virtual bool IsCommand(string input) {
			if (!input.StartsWith('#'))
				return false;

			return true;
		}

		protected bool EvaluateCommand(string command) {
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
					Console.WriteLine(outputString);
					return true;
				case "multiline":
					multiLine = !multiLine;
					outputString = $"Mode: {(multiLine ? "Multiple lines\nPress Ctrl+Enter to evaluate" : "SingleLine")}";
					Console.WriteLine(outputString);
					return true;
				case "showprogram":
					showProgram = !showProgram;
					outputString = $"{(showProgram ? "S" : "Not s")}howing bound tree";
					Console.WriteLine(outputString);
					return true;
				case "evaluate":
					evaluate = !evaluate;
					outputString = $"{(evaluate ? "E" : "Not e")}valuating bound tree";
					Console.WriteLine(outputString);
					return true;
				case "exit":
					running = false;
					return true;
				default:
					return false;
			}
		}

		protected virtual bool IsCompleteSubmission(string input) {
			if (string.IsNullOrEmpty(input))
				return false;

			return true;
		}
	}
}
