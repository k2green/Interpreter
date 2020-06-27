using InterpreterLib.Binding.Types;

namespace InterpreterLib.Binding.Tree {
	internal sealed class BoundAssignmentStatement : BoundStatement {

		public override NodeType Type => NodeType.AssignmentStatement;

		public bool IsDeclaration { get; }

		public BoundNode AssignmentIdentifier { get; }
		public BoundExpression Expression { get; }

		public VariableSymbol Identifier { get;	}

		public BoundAssignmentStatement(BoundNode assignmentNode, BoundExpression expression, VariableSymbol variable) {
			AssignmentIdentifier = assignmentNode;
			Expression = expression;

			Identifier = variable;
		}
	}
}
