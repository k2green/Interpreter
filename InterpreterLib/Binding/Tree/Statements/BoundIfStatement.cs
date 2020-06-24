using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Binding.Tree {
	internal sealed class BoundIfStatement : BoundStatement {

		public override NodeType Type => NodeType.If;

		public BoundExpression Condition { get; }
		public BoundNode TrueBranch { get; }
		public BoundNode FalseBranch { get; }

		public BoundIfStatement(BoundExpression condition, BoundNode trueBranch, BoundNode falseBranch) {
			Condition = condition;
			TrueBranch = trueBranch;
			FalseBranch = falseBranch;
		}
	}
}
