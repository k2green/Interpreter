using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Binding.Tree {
	internal sealed class BoundIfStatement : BoundStatement {

		public override NodeType Type => NodeType.If;

		public BoundExpression Condition { get; }
		public BoundStatement TrueBranch { get; }
		public BoundStatement FalseBranch { get; }

		public BoundIfStatement(BoundExpression condition, BoundStatement trueBranch, BoundStatement falseBranch) {
			Condition = condition;
			TrueBranch = trueBranch;
			FalseBranch = falseBranch;
		}
	}
}
