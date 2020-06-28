using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Syntax.Tree {
	internal enum SyntaxType {
		Literal,
		Error,
		UnaryExpression,
		Token,
		BinaryExpression,
		Declaration,
		Variable,
		TypeDefinition,
		Assignment,
		Expression,
		IfStatement,
		WhileLoop,
		ForLoop
	}
}
