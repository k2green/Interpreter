namespace InterpreterLib.Binding.Tree {
	public enum NodeType {
		UnaryExpression,
		BinaryExpression,
		AssignmentExpression,
		Accessor,
		Variable,
		Literal,

		Block,
		If,
		While,
		For,
		VariableDeclaration,
		Expression,

		Break,
		Continue,

		Label,
		ConditionalBranch,
		Branch,

		Error,

		TypeDefinition,
		FunctionCall,
		InternalTypeConversion,
		FunctionDefinition,
		Tuple,
		Return,
		FunctionPointer,
	}
}
