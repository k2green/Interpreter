using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Text;

namespace Interpreter {
	public sealed class SubmissionView {

		private ObservableCollection<string> submissionText;
		private readonly int top;
		private int renderHeight;

		private int cursorLine;
		private int cursorCharacter;

		public SubmissionView(ObservableCollection<string> text) {
			submissionText = text;
			submissionText.CollectionChanged += SubmissionTextChanged;
			top = Console.CursorTop;
		}

		private void SubmissionTextChanged(object sender, NotifyCollectionChangedEventArgs e) {
			Render();
		}

		private void Render() {
			cursorLine = Console.CursorTop;
			cursorCharacter = Console.CursorLeft;

			Console.CursorVisible = false;
			Console.SetCursorPosition(0, top);

			int lineCount = 0;

			foreach (var line in submissionText) {
				if (lineCount == 0)
					Console.Write("> ");
				else
					Console.Write(". ");

				Console.WriteLine(line);
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
			Console.SetCursorPosition(cursorCharacter, cursorLine);
		}

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
			get => cursorLine;
			set {
				if (cursorCharacter != value) {
					cursorCharacter = value;
					UpdateCursorPosition();
				}
			}
		}
	}
}
