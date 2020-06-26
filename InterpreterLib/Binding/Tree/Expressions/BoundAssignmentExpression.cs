using InterpreterLib.Binding.Types;

namespace InterpreterLib.Binding.Tree {
	internal sealed class BoundAssignmentExpression : BoundExpression {

		public override TypeSymbol ValueType => Expression.ValueType;
		public override NodeType Type => NodeType.AssignmentExpression;

		public VariableSymbol Identifier { get; }
		public BoundExpression Expression { get; }

		public BoundAssignmentExpression(VariableSymbol identifier, BoundExpression expression) {
			Identifier = identifier;
			Expression = expression;
		}
	}
}
