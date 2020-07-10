using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace InterpreterLib.Output {
	public class OutputDocumentBuilder {
		private List<List<OutputFragment>> lines;
		private List<OutputFragment> currentLine;
		private StringBuilder currentText;
		private Color currentColor;

		public OutputDocumentBuilder() {
			lines = new List<List<OutputFragment>>();
			currentLine = new List<OutputFragment>();
			currentText = new StringBuilder();
	}

		public void AddFragment(OutputFragment fragment) {
			if (!currentColor.Equals(fragment.TextColour) && currentText.Length > 0) {
				AddText();
			}

			currentText.Append(fragment.Text);
			currentColor = fragment.TextColour;
		}

		public void NewLine() {
			if (currentText.Length > 0)
				AddText();

			lines.Add(currentLine);
			currentLine = new List<OutputFragment>();
		}

		public void AddText() {
			currentLine.Add(new OutputFragment(currentText.ToString(), currentColor));

			currentText = new StringBuilder();
			currentColor = default(Color);
		}

		public void AddLine(IEnumerable<OutputFragment> fragments) {
			foreach (var fragment in fragments)
				AddFragment(fragment);

			NewLine();
		}

		public OutputDocument ToDocument() {
			if (currentLine.Count > 0)
				NewLine();

			return new OutputDocument(lines);
		}
	}
}
