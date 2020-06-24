using InterpreterLib.Binding.Types;

namespace InterpreterLib.Binding.Tree {
	internal sealed class BoundVariableExpression : BoundExpression {

		public override BoundType ValueType => Variable.ValueType;

		public override NodeType Type => NodeType.Variable;

		public BoundVariable Variable { get; }

		public BoundVariableExpression(BoundVariable variable) {
			Variable = variable;
		}
	}
}
