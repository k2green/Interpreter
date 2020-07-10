using InterpreterLib.Types;
using InterpreterLib.Runtime;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using InterpreterLib.Syntax.Tree;

namespace Interpreter {
	public abstract class Repl {

		protected const int TAB_SPACES = 4;
		protected static readonly string TAB_STRING = new string(' ', TAB_SPACES);

		private List<string> previousSubmissions = new List<string>();
		private int previousSubmissionIndex;

		protected SyntaxTree tree;
		protected bool done;
		protected bool running;

		protected bool multiLine;

		protected Dictionary<VariableSymbol, object> variables;
		protected BindingEnvironment environment;

		public Repl(bool isMultiline) {
			multiLine = isMultiline;
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

				previousSubmissions.Add(input);
			}
		}

		private string EditSubmission() {

			var observable = new ObservableCollection<string>() { "" };
			var view = new SubmissionView(RenderLine, observable);

			done = false;
			previousSubmissionIndex = previousSubmissions.Count;

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
					case ConsoleKey.Tab:
						HandleTab(observable, view);
						break;
					case ConsoleKey.Backspace:
						HandleBackspace(observable, view);
						break;
					case ConsoleKey.PageUp:
						HandlePageUp(observable, view);
						break;
					case ConsoleKey.PageDown:
						HandlePageDown(observable, view);
						break;
					case ConsoleKey.Delete:
						HandleDelete(observable, view);
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

		private void HandleDelete(ObservableCollection<string> observable, SubmissionView view) {
			int line = view.CursorLine;
			int column = view.CursorCharacter;
			int lineLength = observable[line].Length;

			if (column < lineLength) {
				if (column % TAB_SPACES == 0 && column + TAB_SPACES <= lineLength && observable[line].Substring(column, TAB_SPACES).Equals(TAB_STRING)) {
					observable[line] = observable[line].Remove(column, TAB_SPACES);
				} else {
					string text = observable[line].Remove(column, 1);
					observable[line] = text;
				}
			} else {
				int nextLine = line + 1;

				if (nextLine < observable.Count) {
					if (!string.IsNullOrEmpty(observable[nextLine])) {
						observable[view.CursorLine] = observable[line] + observable[nextLine];
						observable.RemoveAt(nextLine);
					} else {
						observable.RemoveAt(nextLine);
					}
				}
			}
		}

		private void HandlePageDown(ObservableCollection<string> observable, SubmissionView view) {
			previousSubmissionIndex++;

			if (previousSubmissionIndex < previousSubmissions.Count) {
				var next = previousSubmissions[previousSubmissionIndex];
				var lines = next.Split(Environment.NewLine);

				if (lines.Length > 1 && !multiLine)
					HandlePageDown(observable, view);
				else
					UpdateFromHistory(observable, view, lines);
			} else {
				previousSubmissionIndex = previousSubmissions.Count;
				observable.Clear();
				observable.Add("");
				view.CursorLine = 0;
				view.CursorCharacter = 0;
			}
		}

		private void HandlePageUp(ObservableCollection<string> observable, SubmissionView view) {
			previousSubmissionIndex--;

			if (previousSubmissionIndex >= 0) {
				var previous = previousSubmissions[previousSubmissionIndex];
				var lines = previous.Split(Environment.NewLine);

				if (lines.Length > 1 && !multiLine)
					HandlePageUp(observable, view);
				else
					UpdateFromHistory(observable, view, lines);
			} else {
				previousSubmissionIndex = -1;
				observable.Clear();
				observable.Add("");
				view.CursorLine = 0;
				view.CursorCharacter = 0;
			}
		}

		private void UpdateFromHistory(ObservableCollection<string> observable, SubmissionView view, string[] lines) {
			observable.Clear();

			foreach (var line in lines)
				observable.Add(line);

			view.CursorLine = lines.Length - 1;
			view.CursorCharacter = observable[view.CursorLine].Length;
		}

		private void HandleTab(ObservableCollection<string> observable, SubmissionView view) {
			int line = view.CursorLine;
			int start = view.CursorCharacter;

			int spaceCount = TAB_SPACES - (start % TAB_SPACES);
			var spaceStr = new string(' ', spaceCount);

			observable[line] = observable[line].Insert(start, spaceStr);
			view.CursorCharacter += spaceStr.Length;
		}

		private void HandleCtrlEnter(ObservableCollection<string> observable, SubmissionView view) {
			done = true;
		}

		private void HandleBackspace(ObservableCollection<string> observable, SubmissionView view) {
			int line = view.CursorLine;
			int column = view.CursorCharacter;

			if (column > 0) {
				if (column % TAB_SPACES == 0 && observable[line].Substring(column - TAB_SPACES, TAB_SPACES).Equals(TAB_STRING)) {
					observable[line] = observable[line].Remove(column - TAB_SPACES, TAB_SPACES);
					view.CursorCharacter -= TAB_SPACES;
				} else {
					string text = observable[line].Remove(column - 1, 1);
					observable[line] = text;
					view.CursorCharacter--;
				}
			} else {
				if(!string.IsNullOrEmpty(observable[line]) && line > 0) {
					view.CursorLine--;
					view.CursorCharacter = observable[view.CursorLine].Length;

					observable[view.CursorLine] = observable[view.CursorLine] + observable[line];
					observable.RemoveAt(line);
				} else if (string.IsNullOrEmpty(observable[line]) && observable.Count > 1) {
					observable.RemoveAt(line);
					view.CursorLine--;
					view.CursorCharacter = observable[view.CursorLine].Length;
				}
			}
		}


		private void HandleTyping(ObservableCollection<string> observable, SubmissionView view, string text) {
			int line = view.CursorLine;
			int start = view.CursorCharacter;

			observable[line] = observable[line].Insert(start, text);
			view.CursorCharacter += text.Length;
		}

		private void HandleEnter(ObservableCollection<string> observable, SubmissionView view) {
			if (!multiLine || (observable.Count == 1 && IsCommand(observable[0]))) {
				HandleCtrlEnter(observable, view);
			} else {
				int line = view.CursorLine;
				var lineText = observable[line];
				int spaceCount = 0;

				foreach (var character in lineText) {
					if (character != ' ')
						break;

					spaceCount++;
				}
				int tabCount = spaceCount / TAB_SPACES;

				if (view.CursorCharacter == lineText.Length) {
					observable.Insert(line + 1, new string(' ', tabCount * TAB_SPACES));
				} else {
					string start = lineText.Substring(0, view.CursorCharacter);
					string end = lineText.Substring(view.CursorCharacter, lineText.Length - view.CursorCharacter);
					observable[line] = start;
					observable.Insert(line + 1, new string(' ', tabCount * TAB_SPACES) + end);
				}

				view.CursorLine++;
				view.CursorCharacter = tabCount * TAB_SPACES;
			}
		}

		private void HandleDownArrow(ObservableCollection<string> observable, SubmissionView view) {
			if(!multiLine) {
				HandlePageDown(observable, view);
			} else {
				if (view.CursorLine < observable.Count - 1)
					view.CursorLine++;

				var line = observable[view.CursorLine];
				if (view.CursorCharacter > line.Length)
					view.CursorCharacter = line.Length;
			}
		}

		private void HandleUpArrow(ObservableCollection<string> observable, SubmissionView view) {
			if(!multiLine) {
				HandlePageUp(observable, view);
			} else {
				if (view.CursorLine > 0)
					view.CursorLine--;

				var line = observable[view.CursorLine];
				if (view.CursorCharacter >= line.Length)
					view.CursorCharacter = line.Length;
			}
		}

		private void HandleRightArrow(ObservableCollection<string> observable, SubmissionView view) {
			var line = observable[view.CursorLine];
			if (view.CursorCharacter < line.Length) {
				view.CursorCharacter++;
			} else if (view.CursorLine < observable.Count - 1) {
				view.CursorLine++;
				view.CursorCharacter = 0;
			}
		}

		private void HandleLeftArrow(ObservableCollection<string> observable, SubmissionView view) {
			if (view.CursorCharacter > 0) {
				view.CursorCharacter--;
			} else if (view.CursorLine > 0) {
				view.CursorLine--;
				view.CursorCharacter = observable[view.CursorLine].Length;
			}
		}

		protected abstract void EvaluateInput(string input);

		protected virtual bool IsCommand(string input) {
			if (!input.StartsWith('#'))
				return false;

			return true;
		}

		protected abstract bool EvaluateCommand(string command);

		protected virtual void RenderLine(string line) {
			Console.WriteLine(line);
		}

		protected virtual bool IsCompleteSubmission(string input) {
			if (string.IsNullOrEmpty(input))
				return false;

			return true;
		}
	}
}
