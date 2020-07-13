using InterpreterLib.Types;

namespace InterpreterLib.Binding.Tree.Expressions {
	internal sealed class BoundAssignmentExpression : BoundExpression {

		public override NodeType Type => NodeType.AssignmentExpression;

		public VariableSymbol Identifier { get; }
		public BoundExpression Expression { get; }

		public override TypeSymbol ValueType => Expression.ValueType;

		public BoundAssignmentExpression(VariableSymbol variable, BoundExpression expression) {
			Expression = expression;
			Identifier = variable;
		}

		public override string ToString() => Identifier.Name + "=" + Expression.ToString();
	}
}
