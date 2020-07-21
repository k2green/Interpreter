using InterpreterLib.Binding.Tree.Expressions;

namespace InterpreterLib.Binding.Tree.Statements {
	internal sealed class BoundExpressionStatement : BoundStatement {

		public override NodeType Type => NodeType.Expression;
		public BoundExpression Expression { get; }

		public bool IsMarkedForRewrite { get; private set; } = false;

		public BoundExpressionStatement(BoundExpression expression) {
			Expression = expression;
		}

		public void MarkForRewrite() => IsMarkedForRewrite = true;

		public override string ToString() => Expression.ToString();
	}
}
