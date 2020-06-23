using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Binding.Tree {
	internal sealed class BoundLiteral : BoundExpression {

		public object Value { get; }
		public override Type ValueType { get; }
		public override NodeType Type => NodeType.Literal;

		public BoundLiteral(object value) {
			Value = value;
			ValueType = value.GetType();
		}
	}
}
