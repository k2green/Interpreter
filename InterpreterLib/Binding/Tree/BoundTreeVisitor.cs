using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Binding.Tree {
	internal abstract class BoundTreeVisitor<T> {
		protected T Visit(BoundNode node) {
			switch (node.Type) {
				case NodeType.Literal:
					return VisitLiteral((BoundLiteral)node);
				case NodeType.UnaryExpression:
					return VisitUnaryExpression((BoundUnaryExpression)node);
				case NodeType.BinaryExpression:
					return VisitBinaryExpression((BoundBinaryExpression)node);
				case NodeType.AssignmentExpression:
					return VisitAssignmentExpression((BoundAssignmentExpression)node);
				case NodeType.Variable:
					return VisitVariable((BoundVariableExpression)node);
				case NodeType.Expression:
					return VisitExpression((BoundExpressionStatement)node);
				case NodeType.Block:
					return VisitBlock((BoundBlock)node);
				case NodeType.If:
					return VisitIf((BoundIfStatement)node);
				case NodeType.While:
					return VisitWhile((BoundWhileStatement)node);
				case NodeType.VariableDeclaration:
					return VisitVariableDeclaration((BoundDeclarationStatement)node);
				case NodeType.For:
					return VisitForStatement((BoundForStatement)node);
				case NodeType.Error:
					return VisitError((BoundError)node);
				default: throw new Exception("Unimplemented node evaluator");
			}
		}

		protected abstract T VisitLiteral(BoundLiteral literal);
		protected abstract T VisitUnaryExpression(BoundUnaryExpression expression);
		protected abstract T VisitBinaryExpression(BoundBinaryExpression expression);
		protected abstract T VisitAssignmentExpression(BoundAssignmentExpression expression);
		protected abstract T VisitVariable(BoundVariableExpression expression);
		protected abstract T VisitExpression(BoundExpressionStatement statement);
		protected abstract T VisitBlock(BoundBlock block);
		protected abstract T VisitIf(BoundIfStatement statement);
		protected abstract T VisitWhile(BoundWhileStatement statement);
		protected abstract T VisitVariableDeclaration(BoundDeclarationStatement statement);
		protected abstract T VisitForStatement(BoundForStatement statement);
		protected abstract T VisitError(BoundError error);
	}
}
