using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using InterpreterLib.Symbols.Types;

namespace InterpreterLib.Binding.Tree.Expressions {
	internal sealed class BoundTuple : BoundExpression {
		public override TypeSymbol ValueType { get; }

		public override NodeType Type => NodeType.Literal;

		public ImmutableArray<TypeSymbol> Types { get; }

		public BoundTuple(ImmutableArray<TypeSymbol> types, BoundScope parentScope) {
			Types = types;
			ValueType = new TupleSymbol(types, parentScope);
		}

		public override string ToString() {
			var builder = new StringBuilder().Append("(");

			for (int index = 0; index < Types.Length; index++) {
				builder.Append(Types[index].ToString());

				if (index < Types.Length - 1)
					builder.Append(", ");
			}

			return builder.Append(")").ToString();
		}
	}
}
