using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace InterpreterLib.Binding.Tree.Patterns {
	internal sealed class BoundTupleIdentifier : BoundIdentifierPattern {
		public override NodeType Type => NodeType.TupleIdentifier;

		public ImmutableArray<BoundIdentifierPattern> Identifiers { get; }

		public BoundTupleIdentifier(ImmutableArray<BoundIdentifierPattern> identifiers) {
			Identifiers = identifiers;
		}

		public override string ToString() {
			var builder = new StringBuilder();

			builder.Append("(");

			for (int index = 0; index < Identifiers.Length; index++) {
				builder.Append(Identifiers[index]);

				if (index < Identifiers.Length - 1)
					builder.Append(", ");
			}

			builder.Append(")");
			return builder.ToString();
		}
	}
}
