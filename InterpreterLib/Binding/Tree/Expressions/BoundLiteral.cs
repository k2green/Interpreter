using InterpreterLib.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Binding.Tree.Expressions {
	internal sealed class BoundLiteral : BoundExpression {

		public object Value { get; }
		public override TypeSymbol ValueType { get; }
		public override NodeType Type => NodeType.Literal;

		public BoundLiteral(object value, TypeSymbol type) {
			Value = value;
			ValueType = type;
		}

		public override string ToString() {
			var outString = Value.ToString();

			if (ValueType == TypeSymbol.String) {
				outString = StringFormat(outString);
			}

			return outString;
		}
	}
}
