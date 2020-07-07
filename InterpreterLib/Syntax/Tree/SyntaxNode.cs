using System;
using System.Collections.Generic;
using System.Linq;

namespace InterpreterLib.Syntax.Tree {
	internal abstract class SyntaxNode {
		public abstract SyntaxType Type { get; }
		public abstract IEnumerable<SyntaxNode> Children { get; }
		public abstract TextLocation Location { get; }

		public override string ToString() {
			var enumerable = from child in Children
							 select child.ToString();

			return string.Join(null, enumerable);
		}
	}
}