using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Binding.Tree {
	internal sealed class BoundDeclarationStatement : BoundStatement {

		public override NodeType Type => NodeType.VariableDeclaration;
		public BoundVariableExpression VariableExpression { get; }

		public BoundDeclarationStatement(BoundVariableExpression variableExpression) {
			VariableExpression = variableExpression;
		}
	}
}
