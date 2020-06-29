using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Syntax.Tree {
	internal abstract class SyntaxNode {
		public abstract SyntaxType Type { get; }
		public abstract IEnumerable<SyntaxNode> Children { get; }
		public abstract TextSpan Span { get; }

		public abstract override string ToString();

		protected TextSpan CreateNewSpan(TextSpan start, TextSpan end) {
			int width = end.Start + end.Width - start.Start;
			return new TextSpan(start.Start, width, start.Line, start.Column);
		}
	}
}