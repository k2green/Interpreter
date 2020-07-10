using InterpreterLib.Binding.Tree.Expressions;
using InterpreterLib.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Binding.Tree.Statements {
	internal sealed class BoundWhileStatement : BoundLoopStatement {

		public override NodeType Type => NodeType.While;

		public BoundExpression Condition { get; }
		public BoundStatement Body { get; }
		public bool AddContinue { get; }

		public override LabelSymbol BreakLabel { get; }
		public override LabelSymbol ContinueLabel { get; }

		public BoundWhileStatement(BoundExpression condition, BoundStatement body, LabelSymbol breakLabel, LabelSymbol continueLabel, bool addContinue = true) {
			Condition = condition;
			Body = body;
			BreakLabel = breakLabel;
			ContinueLabel = continueLabel;
			AddContinue = addContinue;
		}
	}
}
