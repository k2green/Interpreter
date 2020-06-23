using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Binding.Tree {
	internal sealed class BoundVariableExpression : BoundExpression {

		public override Type ValueType => Variable.ValueType;

		public override NodeType Type => NodeType.Variable;

		public BoundVariable Variable { get; }

		public BoundVariableExpression(BoundVariable variable) {
			Variable = variable;
		}
	}
}
