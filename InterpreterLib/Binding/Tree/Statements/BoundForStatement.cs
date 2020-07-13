using InterpreterLib.Binding.Tree.Expressions;
using InterpreterLib.Symbols.Binding;
using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Binding.Tree.Statements {
	internal sealed class BoundForStatement : BoundLoopStatement {

		public override NodeType Type => NodeType.For;

		public BoundStatement Assignment { get; }
		public BoundExpression Condition { get; }
		public BoundExpression Step { get; }
		public BoundStatement Body { get; }

		public override LabelSymbol BreakLabel { get; }
		public override LabelSymbol ContinueLabel { get; }

		public BoundForStatement(BoundStatement assignment, BoundExpression condition, BoundExpression step, BoundStatement body, LabelSymbol breakLabel, LabelSymbol continueLabel) {
			Assignment = assignment;
			Condition = condition;
			Step = step;
			Body = body;
			BreakLabel = breakLabel;
			ContinueLabel = continueLabel;
		}

		public override string ToString() {
			var builder = new StringBuilder();

			builder.Append("for(");
			builder.Append(Assignment.ToString()).Append(", ");
			builder.Append(Condition.ToString()).Append(", ");
			builder.Append(Step.ToString()).Append(") {").Append(Environment.NewLine);
			builder.Append(Body.ToString()).Append(Environment.NewLine);
			builder.Append(Condition.ToString()).Append("}");

			return builder.ToString();
		}
	}
}
