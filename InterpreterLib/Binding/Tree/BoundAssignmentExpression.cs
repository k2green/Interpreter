using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Binding.Tree {
	internal sealed class BoundAssignmentExpression : BoundExpression {

		public override Type ValueType => Expression.ValueType;
		public override NodeType Type => NodeType.AssignmentExpression;

		public BoundVariable Identifier { get; }
		public BoundExpression Expression { get; }

		public BoundAssignmentExpression(BoundVariable identifier, BoundExpression expression) {
			Identifier = identifier;
			Expression = expression;
		}
	}
}
