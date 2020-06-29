using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Syntax {
	public struct TextSpan {
		public TextSpan(int start, int width, int line, int column) {
			Start = start;
			Width = width;
			Line = line;
			Column = column;
		}

		public int Start { get; }
		public int Width { get; }
		public int Line { get; }
		public int Column { get; }
	}
}
