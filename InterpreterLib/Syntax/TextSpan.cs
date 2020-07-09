using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Syntax {
	public struct TextSpan {

		public TextSpan(int start, int end) {
			Start = start;
			End = end;
		}

		public int Start { get; }
		public int End { get; }
		public int Width => End - Start + 1;
	}
}
