using System;
using System.Collections.Generic;
using System.Linq;

namespace InterpreterLib.Syntax.Tree {
	internal abstract class SyntaxNode {
		public abstract SyntaxType Type { get; }
		public abstract IEnumerable<SyntaxNode> Children { get; }
		public abstract TextLocation Span { get; }

		protected TextLocation CreateNewSpan(TextLocation start, TextLocation end) {
			int width = end.Start + end.Width - start.Start;
			return new TextLocation(start.Start, width, start.Line, start.Column);
		}

		public override string ToString() {
			var enumerable = from child in Children
							 select child.ToString();

			return string.Join(null, enumerable);
		}
	}
}