using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace InterpreterLib.Output {
	public struct OutputFragment {

		public string Text { get; }
		public Color TextColour { get; }

		public OutputFragment(string text, Color textColour) {
			Text = text;
			TextColour = textColour;
		}
	}
}
