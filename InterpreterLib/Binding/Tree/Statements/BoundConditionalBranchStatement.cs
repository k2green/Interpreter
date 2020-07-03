using InterpreterLib.Binding.Tree.Expressions;
using InterpreterLib.Binding.Tree.Statements;
using InterpreterLib.Binding.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Binding.Tree.Statements {
	internal sealed class BoundConditionalBranchStatement : BoundStatement {

		public override NodeType Type => NodeType.ConditionalBranch;
		public LabelSymbol Label { get; }
		public BoundExpression Condition { get; }
		public bool BranchIfTrue { get; }

		public BoundConditionalBranchStatement(LabelSymbol label, BoundExpression condition, bool check) {
			Label = label;
			Condition = condition;
			BranchIfTrue = check;
		}
	}
}
