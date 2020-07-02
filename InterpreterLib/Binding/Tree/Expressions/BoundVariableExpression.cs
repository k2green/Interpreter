using InterpreterLib.Binding.Types;

namespace InterpreterLib.Binding.Tree.Expressions {
	internal sealed class BoundVariableExpression : BoundExpression {

		public override TypeSymbol ValueType => Variable.ValueType;

		public override NodeType Type => NodeType.Variable;

		public VariableSymbol Variable { get; }

		public BoundVariableExpression(VariableSymbol variable) {
			Variable = variable;
		}
	}
}
