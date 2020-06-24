using InterpreterLib.Binding.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Binding.Tree {
	internal sealed class BoundLiteral : BoundExpression {

		public object Value { get; }
		public override BoundType ValueType { get; }
		public override NodeType Type => NodeType.Literal;

		private BoundLiteral(object value) {
			Value = value;
		}

		public BoundLiteral(int value) : this((object) value) {
			ValueType = BoundType.Integer;
		}

		public BoundLiteral(double value) : this((object)value) {
			ValueType = BoundType.Double;
		}

		public BoundLiteral(byte value) : this((object)value) {
			ValueType = BoundType.Byte;
		}

		public BoundLiteral(long value) : this((object)value) {
			ValueType = BoundType.Long;
		}

		public BoundLiteral(bool value) : this((object)value) {
			ValueType = BoundType.Boolean;
		}

		public BoundLiteral(string value) : this((object)value) {
			ValueType = BoundType.String;
		}
	}
}
