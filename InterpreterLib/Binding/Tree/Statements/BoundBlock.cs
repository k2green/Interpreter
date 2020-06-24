using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Binding.Tree {
	internal sealed class BoundBlock : BoundNode {
		public override NodeType Type => NodeType.Block;
		public IReadOnlyList<BoundNode> Statements => statements;

		private List<BoundNode> statements;

		public BoundBlock(IEnumerable<BoundNode> statements) {
			this.statements = new List<BoundNode>();
			this.statements.AddRange(statements);
		}
	}
}
