
using InterpreterLib.Symbols.Binding;
using InterpreterLib.Symbols.Types;

namespace InterpreterLib.Binding.Tree.Expressions {
	internal sealed class BoundVariableExpression : BoundExpression {

		public override ValueTypeSymbol ValueType => Variable.ValueType;

		public override NodeType Type => NodeType.Variable;

		public VariableSymbol Variable { get; }

		public BoundVariableExpression(VariableSymbol variable) {
			Variable = variable;
		}

		public override string ToString() => Variable.Name;
	}
}
