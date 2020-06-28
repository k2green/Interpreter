namespace InterpreterLib.Binding.Tree {
	public enum NodeType {
		Literal, UnaryExpression, BinaryExpression, AssignmentExpression,
		Variable,
		Block,
		If,
		While,
		VariableDeclaration,
		For,
		Error,
		Expression,
		Label,
		ConditionalBranch,
		Branch
	}
}
