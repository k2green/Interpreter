using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Text;

namespace Interpreter {
	public sealed class SubmissionView {

		private ObservableCollection<string> submissionText;
		private int renderHeight;

		private int cursorLine;
		private int cursorCharacter;

		public int Top { get; }

		public SubmissionView(ObservableCollection<string> text) {
			submissionText = text;
			submissionText.CollectionChanged += SubmissionTextChanged;
			Top = Console.CursorTop;
			cursorLine = Top;
			Render();
			UpdateCursorPosition();
		}

		private void SubmissionTextChanged(object sender, NotifyCollectionChangedEventArgs e) {
			Render();
		}

		private void Render() {
			Console.CursorVisible = false;
			Console.SetCursorPosition(0, Top);

			int lineCount = 0;

			foreach (var line in submissionText) {
				Console.ForegroundColor = ConsoleColor.Green;

				if (lineCount == 0)
					Console.Write("> ");
				else
					Console.Write(". ");

				Console.ForegroundColor = ConsoleColor.White;

				Console.WriteLine(line + new string(' ', Console.WindowWidth - line.Length - 2));
				lineCount++;
			}

			int blankCount = renderHeight - lineCount;
			var blankLine = new string(' ', Console.WindowWidth);

			while (blankCount > 0) {
				Console.WriteLine(blankLine);
				blankCount--;
			}

			renderHeight = lineCount;
			UpdateCursorPosition();
			Console.CursorVisible = true;
		}

		private void UpdateCursorPosition() {
			Console.SetCursorPosition(cursorCharacter + 2, cursorLine);
		}

		public int TextLine => CursorLine - Top;

		public int CursorLine {
			get => cursorLine;
			set {
				if (cursorLine != value) {
					cursorLine = value;
					UpdateCursorPosition();
				}
			}
		}

		public int CursorCharacter {
			get => cursorCharacter;
			set {
				if (cursorCharacter != value) {
					cursorCharacter = value;
					UpdateCursorPosition();
				}
			}
		}
	}
}
