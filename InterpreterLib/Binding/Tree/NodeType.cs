namespace InterpreterLib.Binding.Tree {
	public enum NodeType {
		Literal, UnaryExpression, BinaryExpression, AssignmentStatement,
		Variable,
		Block,
		If,
		While,
		VariableDeclaration,
		For,
		Error,
		Expression
	}
}
