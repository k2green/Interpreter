using InterpreterLib.Binding.Tree.Expressions;
using InterpreterLib.Symbols.Binding;

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

		public override string ToString() => $"goto {Label.Name} {(BranchIfTrue ? "if" : "unless")} {Condition.ToString()}";
	}
}
