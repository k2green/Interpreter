using InterpreterLib.Binding.Types;

namespace InterpreterLib.Binding.Tree {
	internal sealed class BoundUnaryExpression : BoundExpression {

		public override TypeSymbol ValueType => Op.OutputType;

		public override NodeType Type => NodeType.UnaryExpression;

		public UnaryOperator Op { get; }
		public BoundExpression Operand { get; }

		public BoundUnaryExpression(UnaryOperator op, BoundExpression operand) {
			Op = op;
			Operand = operand;
		}
	}
}
