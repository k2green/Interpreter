using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using InterpreterLib.Symbols.Types;

namespace InterpreterLib.Binding.Tree.Expressions {
	internal sealed class BoundTuple : BoundExpression {
		public override TypeSymbol ValueType { get; }

		public override NodeType Type => NodeType.Tuple;

		public ImmutableArray<BoundExpression> Expressions { get; }
		public bool IsReadOnly { get; }

		public BoundTuple(ImmutableArray<BoundExpression> expressions, bool isReadOnly = false) {
			Expressions = expressions;
			IsReadOnly = isReadOnly;
			var types = expressions.Select(expr => expr.ValueType).ToImmutableArray();
			ValueType = new TupleSymbol(types);
		}

		public bool IsLast(BoundExpression expression) => Expressions[Expressions.Length - 1].Equals(expression);

		public override string ToString() {
			var builder = new StringBuilder().Append("(");

			for (int index = 0; index < Expressions.Length; index++) {
				builder.Append(Expressions[index].ToString());

				if (index < Expressions.Length - 1)
					builder.Append(", ");
			}

			return builder.Append(")").ToString();
		}
	}
}
