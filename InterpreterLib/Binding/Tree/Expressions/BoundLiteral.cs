using InterpreterLib.Symbols.Types;
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
				ValueType = ValueTypeSymbol.Integer;
			else if(Value is double)
				ValueType = ValueTypeSymbol.Double;
			else if (Value is byte)
				ValueType = ValueTypeSymbol.Byte;
			else if (Value is bool)
				ValueType = ValueTypeSymbol.Boolean;
			else if (Value is string)
				ValueType = ValueTypeSymbol.String;
			else if (Value is char)
				ValueType = ValueTypeSymbol.Character;
		}

		public override string ToString() {
			var outString = Value.ToString();

			if (ValueType == ValueTypeSymbol.String) {
				outString = StringFormat(outString);
			}

			return outString;
		}
	}
}
