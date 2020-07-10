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
		VariableDeclaration,
		Variable,
		TypeDefinition,
		Assignment,
		Expression,
		IfStatement,
		WhileLoop,
		ForLoop,
		Block,
		FunctionCall,
		TypedIdentifier,
		ParameterDefinition,
		FunctionDeclaration,
		CompilationUnit,
		GlobalStatement,
		Continue,
		Break,
		Return
	}
}
