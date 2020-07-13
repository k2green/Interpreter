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

		public override string ToString() {
			var builder = new StringBuilder();

			var statCount = Statements.Length;
			for(int index = 0; index < statCount; index++) {
				builder.Append(Statements[index].ToString());

				if (index < statCount - 1)
					builder.Append(Environment.NewLine);
			}

			return builder.ToString();
		}
	}
}
