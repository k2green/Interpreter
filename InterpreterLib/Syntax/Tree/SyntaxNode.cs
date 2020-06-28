using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Syntax.Tree {
	internal abstract class SyntaxNode {
		public abstract SyntaxType Type { get; }

		public abstract IEnumerable<SyntaxNode> Children { get; }
	}
}
