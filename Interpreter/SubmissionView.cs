using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Text;

namespace Interpreter {
	public sealed class SubmissionView {

		public Action<string> Renderer { get; }

		private ObservableCollection<string> submissionText;
		private int renderHeight;

		private int cursorLine;
		private int cursorCharacter;

		public int Top { get; }

		public SubmissionView(Action<string> renderer, ObservableCollection<string> text) {
			Renderer = renderer;
			submissionText = text;
			submissionText.CollectionChanged += SubmissionTextChanged;
			Top = Console.CursorTop;
			cursorLine = 0;
			Render();
			UpdateCursorPosition();
		}

		private void SubmissionTextChanged(object sender, NotifyCollectionChangedEventArgs e) {
			Render();
		}

		private void Render() {
			Console.CursorVisible = false;
			Console.SetCursorPosition(0, Top);

			var blankLine = new string(' ', Console.WindowWidth);

			for (int blankCount = 0; blankCount < renderHeight; blankCount++) {
				Console.WriteLine(blankLine);
			}

			Console.SetCursorPosition(0, Top);
			int lineCount = 0;

			foreach (var line in submissionText) {
				Console.ForegroundColor = ConsoleColor.Green;

				if (lineCount == 0)
					Console.Write("> ");
				else
					Console.Write(". ");

				Console.ForegroundColor = ConsoleColor.White;

				Renderer(line);
				lineCount++;
			}

			renderHeight = lineCount;
			UpdateCursorPosition();
			Console.CursorVisible = true;
		}

		private void UpdateCursorPosition() {
			Console.SetCursorPosition(cursorCharacter + 2, cursorLine + Top);
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
