using InterpreterLib.Binding.Types;

namespace InterpreterLib.Binding.Tree {
	internal sealed class BoundAssignmentExpression : BoundExpression {

		public override BoundType ValueType => Expression.ValueType;
		public override NodeType Type => NodeType.AssignmentExpression;

		public BoundVariable Identifier { get; }
		public BoundExpression Expression { get; }

		public BoundAssignmentExpression(BoundVariable identifier, BoundExpression expression) {
			Identifier = identifier;
			Expression = expression;
		}
	}
}
