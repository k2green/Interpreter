using InterpreterLib.Binding.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Binding.Tree {
	internal sealed class BoundLiteral : BoundExpression {

		public object Value { get; }
		public override TypeSymbol ValueType { get; }
		public override NodeType Type => NodeType.Literal;

		private BoundLiteral(object value) {
			Value = value;
		}

		public BoundLiteral(int value) : this((object) value) {
			ValueType = TypeSymbol.Integer;
		}

		public BoundLiteral(double value) : this((object)value) {
			ValueType = TypeSymbol.Double;
		}

		public BoundLiteral(byte value) : this((object)value) {
			ValueType = TypeSymbol.Byte;
		}

		public BoundLiteral(bool value) : this((object)value) {
			ValueType = TypeSymbol.Boolean;
		}

		public BoundLiteral(string value) : this((object)value) {
			ValueType = TypeSymbol.String;
		}
	}
}
