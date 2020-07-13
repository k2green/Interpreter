using InterpreterLib.Binding.Tree.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Binding.Tree.Statements {
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

		public override string ToString() {
			var builder = new StringBuilder();

			builder.Append("if(").Append(Condition.ToString()).Append(") {").Append(Environment.NewLine);
			builder.Append(TrueBranch.ToString()).Append(Environment.NewLine);
			builder.Append("}");

			if(FalseBranch != null) {
				builder.Append(" else {").Append(Environment.NewLine);
				builder.Append(FalseBranch.ToString()).Append(Environment.NewLine);
				builder.Append("}");
			}

			return builder.ToString();
		}
	}
}
