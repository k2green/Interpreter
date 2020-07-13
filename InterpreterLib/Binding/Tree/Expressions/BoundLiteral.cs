using InterpreterLib.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Binding.Tree.Expressions {
	internal sealed class BoundLiteral : BoundExpression {

		public object Value { get; }
		public override TypeSymbol ValueType { get; }
		public override NodeType Type => NodeType.Literal;

		public BoundLiteral(object value) {
			Value = value;

			if (Value is int)
				ValueType = TypeSymbol.Integer;
			else if(Value is double)
				ValueType = TypeSymbol.Double;
			else if (Value is byte)
				ValueType = TypeSymbol.Byte;
			else if (Value is bool)
				ValueType = TypeSymbol.Boolean;
			else if (Value is string)
				ValueType = TypeSymbol.String;
			else if (Value is char)
				ValueType = TypeSymbol.Character;
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
