using InterpreterLib.Binding.Tree.Expressions;
using InterpreterLib.Binding.Tree.Statements;
using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Binding.Tree {
	internal class BoundTreeRewriter {

		public BoundNode RewriteNode(BoundNode node) {
			if (node is BoundExpression)
				return RewriteExpression((BoundExpression)node);
			else if (node is BoundStatement)
				return RewriteStatement((BoundStatement)node);
			else
				throw new Exception($"Invalid node {node.Type}");
		}

		protected virtual BoundExpression RewriteExpression(BoundExpression expression) {
			switch (expression.Type) {
				case NodeType.AssignmentExpression:
					return RewriteAssignmentExpression((BoundAssignmentExpression)expression);
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
			return literal;
		}

		protected virtual BoundExpression RewriteUnaryExpression(BoundUnaryExpression expression) {
			var operand = RewriteExpression(expression.Operand);

			if (operand == expression.Operand)
				return expression;

			return new BoundUnaryExpression(expression.Op, operand);
		}

		protected virtual BoundExpression RewriteVariableExpression(BoundVariableExpression expression) {
			return expression;
		}

		protected virtual BoundExpression RewriteBinaryExpression(BoundBinaryExpression expression) {
			var rewritenLeftExpresson = RewriteExpression(expression.LeftExpression);
			var rewritenRightExpresson = RewriteExpression(expression.RightExpression);

			if (rewritenLeftExpresson == expression.LeftExpression && rewritenRightExpresson == expression.RightExpression)
				return expression;

			return new BoundBinaryExpression(rewritenLeftExpresson, expression.Op, rewritenRightExpresson);
		}

		protected virtual BoundExpression RewriteAssignmentExpression(BoundAssignmentExpression statement) {
			var expression = RewriteExpression(statement.Expression);
			if (expression == statement.Expression)
				return statement;

			return new BoundAssignmentExpression(statement.Identifier, expression);
		}

		protected virtual BoundStatement RewriteStatement(BoundStatement statement) {
			switch (statement.Type) {
				case NodeType.Block:
					return RewriteBlock((BoundBlock)statement);
				case NodeType.If:
					return RewriteIfStatement((BoundIfStatement)statement);
				case NodeType.While:
					return RewriteWhileStatement((BoundWhileStatement)statement);
				case NodeType.VariableDeclaration:
					return RewriteVariableDeclaration((BoundVariableDeclarationStatement)statement);
				case NodeType.For:
					return RewriteForStatement((BoundForStatement)statement);
				case NodeType.Expression:
					return RewriteExpressionStatement((BoundExpressionStatement)statement);
				case NodeType.Label:
					return RewriteLabelStatement((BoundLabel)statement);
				case NodeType.ConditionalBranch:
					return RewriteConditionalBranchStatement((BoundConditionalBranchStatement)statement);
				case NodeType.Branch:
					return RewriteBranchStatement((BoundBranchStatement)statement);
				default: throw new Exception($"Unexpected statement {statement.Type}");
			}
		}

		private BoundStatement RewriteBranchStatement(BoundBranchStatement statement) {
			var label = RewriteLabelStatement(statement.Label);

			if (label == statement.Label)
				return statement;

			return new BoundBranchStatement(label);
		}

		private BoundStatement RewriteConditionalBranchStatement(BoundConditionalBranchStatement statement) {
			var label = RewriteLabelStatement(statement.Label);
			var condition = RewriteExpression(statement.Condition);

			if (label == statement.Label && condition == statement.Condition)
				return statement;

			return new BoundConditionalBranchStatement(label, condition, statement.Check);
		}

		private BoundLabel RewriteLabelStatement(BoundLabel statement) {
			return statement;
		}

		private BoundStatement RewriteExpressionStatement(BoundExpressionStatement statement) {
			var expression = RewriteExpression(statement.Expression);

			if (expression == statement.Expression)
				return statement;

			return new BoundExpressionStatement(expression);
		}

		protected virtual BoundStatement RewriteBlock(BoundBlock block) {
			List<BoundStatement> rewrittenStatements = new List<BoundStatement>();
			bool isSame = true;

			foreach (var stat in block.Statements) {
				var rewritten = RewriteStatement(stat);

				if (rewritten != stat)
					isSame = false;

				rewrittenStatements.Add(rewritten);
			}

			if (isSame)
				return block;

			return new BoundBlock(rewrittenStatements);
		}

		protected virtual BoundNode RewriteError(BoundError error) {
			throw new NotImplementedException();
		}

		protected virtual BoundStatement RewriteForStatement(BoundForStatement statement) {
			var assignment = RewriteStatement(statement.Assignment);
			var cond = RewriteExpression(statement.Condition);
			var step = RewriteExpression(statement.Step);

			var body = RewriteStatement(statement.Body);

			if (assignment == statement.Assignment && cond == statement.Condition && step == statement.Step && body == statement.Body)
				return statement;

			return new BoundForStatement(assignment, cond, step, body);
		}

		protected virtual BoundStatement RewriteIfStatement(BoundIfStatement statement) {
			var cond = RewriteExpression(statement.Condition);

			BoundStatement trueBranch = RewriteStatement(statement.TrueBranch);
			BoundStatement falseBranch = statement.FalseBranch == null ? null : RewriteStatement(statement.FalseBranch);

			if (cond == statement.Condition && trueBranch == statement.TrueBranch && falseBranch == statement.FalseBranch)
				return statement;

			return new BoundIfStatement(cond, trueBranch, falseBranch);
		}

		protected virtual BoundStatement RewriteVariableDeclaration(BoundVariableDeclarationStatement statement) {
			return statement;
		}

		protected virtual BoundStatement RewriteWhileStatement(BoundWhileStatement statement) {
			var cond = RewriteExpression(statement.Condition);
			var body = RewriteStatement(statement.Body);

			if (cond == statement.Condition && body == statement.Body)
				return statement;

			return new BoundWhileStatement(cond, body);
		}
	}
}
