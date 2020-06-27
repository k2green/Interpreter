using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Binding.Tree {
	internal sealed class BoundBlock : BoundStatement {
		public override NodeType Type => NodeType.Block;
		public IReadOnlyList<BoundStatement> Statements => statements;

		private List<BoundStatement> statements;

		public BoundBlock(IEnumerable<BoundStatement> statements) {
			this.statements = new List<BoundStatement>();
			this.statements.AddRange(statements);
		}
	}
}
