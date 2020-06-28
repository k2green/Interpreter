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
				case NodeType.AssignmentStatement:
					return VisitAssignmentExpression((BoundAssignmentExpression)node);
				case NodeType.Variable:
					return VisitVariable((BoundVariableExpression)node);
				case NodeType.Block:
					return VisitBlock((BoundBlock)node);
				case NodeType.If:
					return VisitIf((BoundIfStatement)node);
				case NodeType.While:
					return VisitWhile((BoundWhileStatement)node);
				case NodeType.VariableDeclaration:
					return VisitVariableDeclaration((BoundVariableDeclarationStatement)node);
				case NodeType.For:
					return VisitForStatement((BoundForStatement)node);
				case NodeType.Error:
					return VisitError((BoundError)node);
				default: throw new Exception("Unimplemented node evaluator");
			}
		}

		protected abstract T VisitExpressionStatement(BoundExpressionStatement statement);
		protected abstract T VisitLiteral(BoundLiteral literal);
		protected abstract T VisitUnaryExpression(BoundUnaryExpression expression);
		protected abstract T VisitBinaryExpression(BoundBinaryExpression expression);
		protected abstract T VisitAssignmentExpression(BoundAssignmentExpression expression);
		protected abstract T VisitVariable(BoundVariableExpression expression);
		protected abstract T VisitBlock(BoundBlock block);
		protected abstract T VisitIf(BoundIfStatement statement);
		protected abstract T VisitWhile(BoundWhileStatement statement);
		protected abstract T VisitVariableDeclaration(BoundVariableDeclarationStatement statement);
		protected abstract T VisitForStatement(BoundForStatement statement);
		protected abstract T VisitError(BoundError error);
	}


	internal abstract class BoundTreeVisitor<T, U> where U : T {
		protected T Visit(BoundNode node) {
			switch (node.Type) {
				case NodeType.Literal:
					return VisitLiteral((BoundLiteral)node);
				case NodeType.UnaryExpression:
					return VisitUnaryExpression((BoundUnaryExpression)node);
				case NodeType.BinaryExpression:
					return VisitBinaryExpression((BoundBinaryExpression)node);
				case NodeType.AssignmentStatement:
					return VisitAssignmentExpression((BoundAssignmentExpression)node);
				case NodeType.Variable:
					return VisitVariable((BoundVariableExpression)node);
				case NodeType.Block:
					return VisitBlock((BoundBlock)node);
				case NodeType.If:
					return VisitIf((BoundIfStatement)node);
				case NodeType.While:
					return VisitWhile((BoundWhileStatement)node);
				case NodeType.VariableDeclaration:
					return VisitVariableDeclaration((BoundVariableDeclarationStatement)node);
				case NodeType.For:
					return VisitForStatement((BoundForStatement)node);
				case NodeType.Error:
					return VisitError((BoundError)node);
				case NodeType.Expression:
					return VisitExpressionStatement((BoundExpressionStatement) node);
				default: throw new Exception("Unimplemented node evaluator");
			}
		}

		protected abstract U VisitExpressionStatement(BoundExpressionStatement statement);
		protected abstract U VisitLiteral(BoundLiteral literal);
		protected abstract U VisitUnaryExpression(BoundUnaryExpression expression);
		protected abstract U VisitBinaryExpression(BoundBinaryExpression expression);
		protected abstract U VisitAssignmentExpression(BoundAssignmentExpression expression);
		protected abstract U VisitVariable(BoundVariableExpression expression);
		protected abstract T VisitBlock(BoundBlock block);
		protected abstract T VisitIf(BoundIfStatement statement);
		protected abstract T VisitWhile(BoundWhileStatement statement);
		protected abstract T VisitVariableDeclaration(BoundVariableDeclarationStatement statement);
		protected abstract T VisitForStatement(BoundForStatement statement);
		protected abstract T VisitError(BoundError error);
	}

	internal abstract class BoundTreeVisitor<T, U, V> {
		protected T Visit(BoundNode node, U param1, V param2) {
			switch (node.Type) {
				case NodeType.Literal:
					return VisitLiteral((BoundLiteral)node, param1, param2);
				case NodeType.UnaryExpression:
					return VisitUnaryExpression((BoundUnaryExpression)node, param1, param2);
				case NodeType.BinaryExpression:
					return VisitBinaryExpression((BoundBinaryExpression)node, param1, param2);
				case NodeType.AssignmentStatement:
					return VisitAssignmentExpression((BoundAssignmentExpression)node, param1, param2);
				case NodeType.Variable:
					return VisitVariable((BoundVariableExpression)node, param1, param2);
				case NodeType.Block:
					return VisitBlock((BoundBlock)node, param1, param2);
				case NodeType.If:
					return VisitIf((BoundIfStatement)node, param1, param2);
				case NodeType.While:
					return VisitWhile((BoundWhileStatement)node, param1, param2);
				case NodeType.VariableDeclaration:
					return VisitVariableDeclaration((BoundVariableDeclarationStatement)node, param1, param2);
				case NodeType.For:
					return VisitForStatement((BoundForStatement)node, param1, param2);
				case NodeType.Error:
					return VisitError((BoundError)node, param1, param2);
				case NodeType.Expression:
					return VisitExpressionStatement((BoundExpressionStatement)node, param1, param2);
				default: throw new Exception("Unimplemented node evaluator");
			}
		}

		protected abstract T VisitExpressionStatement(BoundExpressionStatement statement, U param1, V param2);
		protected abstract T VisitLiteral(BoundLiteral literal, U param1, V param2);
		protected abstract T VisitUnaryExpression(BoundUnaryExpression expression, U param1, V param2);
		protected abstract T VisitBinaryExpression(BoundBinaryExpression expression, U param1, V param2);
		protected abstract T VisitAssignmentExpression(BoundAssignmentExpression expression, U param1, V param2);
		protected abstract T VisitVariable(BoundVariableExpression expression, U param1, V param2);
		protected abstract T VisitBlock(BoundBlock block, U param1, V param2);
		protected abstract T VisitIf(BoundIfStatement statement, U param1, V param2);
		protected abstract T VisitWhile(BoundWhileStatement statement, U param1, V param2);
		protected abstract T VisitVariableDeclaration(BoundVariableDeclarationStatement statement, U param1, V param2);
		protected abstract T VisitForStatement(BoundForStatement statement, U param1, V param2);
		protected abstract T VisitError(BoundError error, U param1, V param2);
	}
}
