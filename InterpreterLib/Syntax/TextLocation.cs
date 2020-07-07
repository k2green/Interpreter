using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Syntax {
	public struct TextLocation {

		public int Line { get; }
		public int Column { get; }

		public TextLocation(int line, int column) {
			Line = line;
			Column = column;
		}
	}
}
