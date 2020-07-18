using System;
using System.Collections.Generic;
using System.Text;
using InterpreterLib.Symbols.Types;

namespace InterpreterLib.Binding.Tree.Expressions {
	internal sealed class BoundAccessor : BoundExpression {
		public override TypeSymbol ValueType { get; }

		public override NodeType Type => NodeType.Accessor;

		public BoundExpression Item { get; }
		public BoundExpression Index { get; }
		public BoundAccessor Rest { get; }
		public bool IsLast => Rest == null;

		public BoundAccessor(BoundExpression expression, BoundExpression index, BoundAccessor rest) {
			Item = expression;
			Index = index;
			Rest = rest;

			if(Index != null && Item.ValueType is ArraySymbol arraySymbol) {
				ValueType = arraySymbol.ValueType;
			} else {
				ValueType = Item.ValueType;
			}
		}

		public override string ToString() {
			var builder = new StringBuilder();

			builder.Append(Item);

			if (Index != null)
				builder.Append("[").Append(Index).Append("]");

			if (!IsLast)
				builder.Append(".").Append(Rest);

			return builder.ToString();
		}
	}
}
