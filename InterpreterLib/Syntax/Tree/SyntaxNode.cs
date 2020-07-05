using System;
using System.Collections.Generic;
using System.Linq;

namespace InterpreterLib.Syntax.Tree {
	internal abstract class SyntaxNode {
		public abstract SyntaxType Type { get; }
		public abstract IEnumerable<SyntaxNode> Children { get; }
		public abstract TextSpan Span { get; }

		protected TextSpan CreateNewSpan(TextSpan start, TextSpan end) {
			int width = end.Start + end.Width - start.Start;
			return new TextSpan(start.Start, width, start.Line, start.Column);
		}

		public override string ToString() {
			var enumerable = from child in Children
							 select child.ToString();

			return string.Join(null, enumerable);
		}
	}
}