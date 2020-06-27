using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Binding.Tree {
	internal class BoundTreeRewriter {

		protected virtual BoundStatement RewriteStatement(BoundStatement statement) {
			switch (statement.Type) {
				case NodeType.AssignmentStatement:
					return RewriteAssignmentStatement((BoundAssignmentStatement)statement);
				case NodeType.Block:
					return RewriteBlock((BoundBlock)statement);
				case NodeType.If:
					return RewriteIfStatement((BoundIfStatement)statement);
				case NodeType.While:
					return RewriteWhileStatement((BoundWhileStatement)statement);
				case NodeType.VariableDeclaration:
					return RewriteVariableDeclaration((BoundDeclarationStatement)statement);
				case NodeType.For:
					return RewriteForStatement((BoundForStatement)statement);

				default: throw new Exception($"Unexpected statement {statement.Type}");
			}
		}

		protected virtual BoundExpression RewriteExpression(BoundExpression expression) {
			switch (expression.Type) {
				case NodeType.Literal:
					return RewriteLiteral((BoundLiteral)expression);
				case NodeType.UnaryExpression:
					return RewriteUnaryExpression((BoundUnaryExpression)expression);
				case NodeType.BinaryExpression:
					return RewriteBinaryExpression((BoundBinaryExpression)expression);
				case NodeType.Variable:
					return RewriteVariableExpression((BoundVariableExpression)expression);
				default: throw new Exception($"Unexpected expression {expression.Type}");
			}
		}

		protected virtual BoundExpression RewriteLiteral(BoundLiteral literal) {
			throw new NotImplementedException();
		}

		protected virtual BoundExpression RewriteUnaryExpression(BoundUnaryExpression expression) {
			throw new NotImplementedException();
		}

		protected virtual BoundExpression RewriteVariableExpression(BoundVariableExpression expression) {
			throw new NotImplementedException();
		}

		protected virtual BoundExpression RewriteBinaryExpression(BoundBinaryExpression expression) {
			var rewritenLeftExpresson = RewriteExpression(expression.LeftExpression);
			var rewritenRightExpresson = RewriteExpression(expression.RightExpression);

			if (rewritenLeftExpresson == expression.LeftExpression && rewritenRightExpresson == expression.RightExpression)
				return expression;

			return new BoundBinaryExpression(rewritenLeftExpresson, expression.Op, rewritenRightExpresson);
		}

		protected virtual BoundStatement RewriteAssignmentStatement(BoundAssignmentStatement statement) {
			var expression = RewriteExpression(statement.Expression);

			if (statement.IsDeclaration) {
				var declaration = RewriteStatement(statement.DeclarationStatement);

				if (statement.DeclarationStatement == declaration && statement.Expression == expression) 
					return statement;

				else return new BoundAssignmentStatement(declaration, expression)
			} else {

			}
		}

		protected virtual BoundStatement RewriteBlock(BoundBlock block) {
			throw new NotImplementedException();
		}

		protected virtual BoundStatement RewriteError(BoundError error) {
			throw new NotImplementedException();
		}

		protected virtual BoundStatement RewriteForStatement(BoundForStatement statement) {
			throw new NotImplementedException();
		}

		protected virtual BoundStatement RewriteIfStatement(BoundIfStatement statement) {
			throw new NotImplementedException();
		}

		protected virtual BoundStatement RewriteVariableDeclaration(BoundDeclarationStatement statement) {
			throw new NotImplementedException();
		}

		protected virtual BoundStatement RewriteWhileStatement(BoundWhileStatement statement) {
			throw new NotImplementedException();
		}
	}
}
