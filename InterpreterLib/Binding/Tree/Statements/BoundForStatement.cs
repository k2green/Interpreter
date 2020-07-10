using InterpreterLib.Binding.Tree.Expressions;
using InterpreterLib.Types;
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
	}
}
