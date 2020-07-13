using InterpreterLib.Binding.Tree.Expressions;

namespace InterpreterLib.Binding.Tree.Statements {
	internal sealed class BoundExpressionStatement : BoundStatement {

		public override NodeType Type => NodeType.Expression;
		public BoundExpression Expression { get; }

		public BoundExpressionStatement(BoundExpression expression) {
			Expression = expression;
		}

		public override string ToString() => Expression.ToString();
	}
}
