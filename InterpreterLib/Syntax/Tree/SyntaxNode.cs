using System;
using System.Collections.Generic;
using System.Linq;

namespace InterpreterLib.Syntax.Tree {
	internal abstract class SyntaxNode {
		public abstract SyntaxType Type { get; }
		public abstract IEnumerable<SyntaxNode> Children { get; }
		public virtual TextLocation Location => Children.First().Location;
		public virtual TextSpan Span => new TextSpan(Children.First().Span.Start, Children.Last().Span.End);

		public override string ToString() {
			var enumerable = from child in Children
							 select child.ToString();

			return string.Join(null, enumerable);
		}
	}
}