using InterpreterLib.Binding.Types;

namespace InterpreterLib.Binding.Tree {
	internal sealed class BoundAssignmentExpression : BoundExpression {

		public override NodeType Type => NodeType.AssignmentStatement;

		public VariableSymbol Identifier { get; }
		public BoundExpression Expression { get; }

		public override TypeSymbol ValueType => Expression.ValueType;

		public BoundAssignmentExpression(BoundNode assignmentNode, BoundExpression expression, VariableSymbol variable) {
			Expression = expression;
			Identifier = variable;
		}
	}
}
