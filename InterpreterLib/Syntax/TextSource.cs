using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Syntax {
	public class TextSource {

		private string source;

		public TextSource(string sourceText) {
			source = sourceText;
		}

		public char this[int index] => source[index];

		public string GetSubstring(TextSpan? span) {
			TextSpan notNull = span ?? new TextSpan(0, 0);
			if (notNull.Width < 1)
				return "";

			var outStr = source.Substring(notNull.Start, notNull.Width);

			if (outStr.Length == 1 && char.IsLetter(outStr[0]))
				return "";

			return outStr;
		}
	}
}
