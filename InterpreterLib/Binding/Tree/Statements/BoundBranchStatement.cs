using InterpreterLib.Symbols.Binding;

namespace InterpreterLib.Binding.Tree.Statements {
	internal sealed class BoundBranchStatement : BoundStatement {

		public override NodeType Type => NodeType.Branch;
		public LabelSymbol Label { get; }

		public BoundBranchStatement(LabelSymbol label) {
			Label = label;
		}

		public override string ToString() => $"goto {Label.Name}";
	}
}
