using InterpreterLib.Binding.Tree.Expressions;
using InterpreterLib.Binding.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Binding.Tree.Statements {
	internal sealed class BoundVariableDeclarationStatement : BoundStatement {

		public override NodeType Type => NodeType.VariableDeclaration;

		public VariableSymbol Variable;
		public BoundExpression Initialiser;

		public BoundVariableDeclarationStatement(VariableSymbol variable, BoundExpression initialiser) {
			Variable = variable;
			Initialiser = initialiser;
		}
	}
}
