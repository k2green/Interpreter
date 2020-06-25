using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Binding.Tree {
	public enum NodeType {
		Literal, UnaryExpression, BinaryExpression, AssignmentExpression,
		Variable,
		Expression,
		Block,
		If,
		While,
		VariableDeclaration
	}
}
