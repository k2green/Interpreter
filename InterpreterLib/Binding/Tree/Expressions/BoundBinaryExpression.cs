using InterpreterLib.Types;

namespace InterpreterLib.Binding.Tree.Expressions {
	internal sealed class BoundBinaryExpression : BoundExpression {

		public override TypeSymbol ValueType => Op.OutputType;
		public override NodeType Type => NodeType.BinaryExpression;

		public BoundExpression LeftExpression { get; }
		public BinaryOperator Op { get; }
		public BoundExpression RightExpression { get; }

		public BoundBinaryExpression(BoundExpression leftExpression, BinaryOperator op, BoundExpression rightExpression) {
			LeftExpression = leftExpression;
			Op = op;
			RightExpression = rightExpression;
		}
	}
}
