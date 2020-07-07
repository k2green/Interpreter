using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace InterpreterLib.Binding.Tree.Statements {
	internal sealed class BoundBlock : BoundStatement {
		public override NodeType Type => NodeType.Block;

		public ImmutableArray<BoundStatement> Statements { get; }

		public BoundBlock(ImmutableArray<BoundStatement> statements) {
			Statements = statements;
		}
	}
}
