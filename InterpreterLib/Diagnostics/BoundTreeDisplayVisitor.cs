using InterpreterLib.Binding.Tree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InterpreterLib.Diagnostics {
	internal class BoundTreeDisplayVisitor : BoundTreeVisitor<IEnumerable<string>, string, string> {
		public const string NEXT_CHILD = "  ├───";
		public const string NO_CHILD = "  │   ";
		public const string LAST_CHILD = "  └───";
		public const string SPACING = "      ";

		public IEnumerable<string> GetText(BoundNode node) {
			return Visit(node, SPACING, SPACING);
		}

		protected override IEnumerable<string> VisitAssignmentExpression(BoundAssignmentExpression expression, string prefix1, string prefix2) {
			var line = new string[] {
				$"{prefix1}Assignment Expression",
				$"{prefix2 + NEXT_CHILD}Variable: {expression.Identifier}"
			};

			return line.Concat(Visit(expression.Expression, prefix2 + LAST_CHILD, prefix2 + SPACING));
		}

		protected override IEnumerable<string> VisitBinaryExpression(BoundBinaryExpression expression, string prefix1, string prefix2) {
			var op = expression.Op;
			var baseLine = new string[] { $"{prefix1}BinaryExpression {op.TokenText} : ({op.LeftType}, {op.RightType}) => {op.OutputType}" };

			return baseLine.Concat(Visit(expression.LeftExpression, prefix2 + NEXT_CHILD, prefix2 + NO_CHILD))
				.Concat(Visit(expression.RightExpression, prefix2 + LAST_CHILD, prefix2 + SPACING));
		}

		protected override IEnumerable<string> VisitBlock(BoundBlock block, string prefix1, string prefix2) {
			IEnumerable<string> line = new string[] { $"{prefix1}Block" };

			if (block.Statements != null) {
				for (int i = 0; i < block.Statements.Count - 1; i++)
					line = line.Concat(Visit(block.Statements[i], prefix2 + NEXT_CHILD, prefix2 + NO_CHILD));

				line = line.Concat(Visit(block.Statements[block.Statements.Count - 1], prefix2 + LAST_CHILD, prefix2 + SPACING));
			}

			return line;
		}

		protected override IEnumerable<string> VisitError(BoundError error, string prefix1, string prefix2) {
			IEnumerable<string> line = new string[] { $"{prefix1}{error.Error}"};

			if (error.Children != null) {
				var children = error.Children.ToArray();
				for (int i = 0; i < children.Length - 1; i++) {
					line = line.Concat(Visit(children[i], prefix2 + NEXT_CHILD, prefix2 + NO_CHILD));
				}

				line = line.Concat(Visit(children[children.Length - 1], prefix2 + LAST_CHILD, prefix2 + SPACING));
			}

			return line;
		}

		protected override IEnumerable<string> VisitExpressionStatement(BoundExpressionStatement statement, string prefix1, string prefix2) {
			IEnumerable<string> lines = new string[] { $"{prefix1}Expression Statement" };

			return lines.Concat(Visit(statement.Expression, prefix2 + LAST_CHILD, prefix2 + SPACING));
		}

		protected override IEnumerable<string> VisitForStatement(BoundForStatement statement, string prefix1, string prefix2) {
			IEnumerable<string> lines = new string[] { $"{prefix1}For Statement" };

			lines = lines.Concat(Visit(statement.Assignment, prefix2 + NEXT_CHILD + "assignment=", prefix2 + NO_CHILD));
			lines = lines.Concat(Visit(statement.Condition, prefix2 + NEXT_CHILD + "condition=", prefix2 + NO_CHILD));
			lines = lines.Concat(Visit(statement.Step, prefix2 + NEXT_CHILD + "step=", prefix2 + NO_CHILD));
			return lines.Concat(Visit(statement.Body, prefix2 + LAST_CHILD, prefix2 + SPACING));
		}

		protected override IEnumerable<string> VisitIf(BoundIfStatement statement, string prefix1, string prefix2) {
			IEnumerable<string> line = new string[] { $"{prefix1}For Statement" };

			line = line.Concat(Visit(statement.Condition, prefix2 + NEXT_CHILD, prefix2 + NO_CHILD));

			if (statement.FalseBranch != null) {
				line = line.Concat(Visit(statement.TrueBranch, prefix2 + NEXT_CHILD, prefix2 + NO_CHILD));
				line = line.Concat(Visit(statement.FalseBranch, prefix2 + LAST_CHILD, prefix2 + SPACING));
			} else {
				line = line.Concat(Visit(statement.TrueBranch, prefix2 + LAST_CHILD, prefix2 + SPACING));
			}

			return line;
		}

		protected override IEnumerable<string> VisitLiteral(BoundLiteral literal, string prefix1, string prefix2) {
			return new string[] { $"{prefix1}Literal {literal.Value} : {literal.ValueType}" };
		}

		protected override IEnumerable<string> VisitUnaryExpression(BoundUnaryExpression expression, string prefix1, string prefix2) {
			var op = expression.Op;
			var baseLine = new string[] { $"{prefix1}Unary expression {op.TokenText} : {op.OperandType} => {op.OutputType}" };

			return baseLine.Concat(Visit(expression.Operand, prefix2 + LAST_CHILD, prefix2 + SPACING));
		}

		protected override IEnumerable<string> VisitVariable(BoundVariableExpression expression, string prefix1, string prefix2) {
			return new string[] { $"{prefix1}Variable {expression.Variable}" };
		}

		protected override IEnumerable<string> VisitVariableDeclaration(BoundVariableDeclarationStatement statement, string prefix1, string prefix2) {
			var baseLine = new string[] { $"{prefix1}Variable Declaration" };

			return baseLine.Concat(Visit(statement.Initialiser, prefix2 + LAST_CHILD, prefix2 + SPACING));
		}

		protected override IEnumerable<string> VisitWhile(BoundWhileStatement statement, string prefix1, string prefix2) {
			IEnumerable<string> lines = new string[] { $"{prefix1}While Statement" };

			lines = lines.Concat(Visit(statement.Condition, prefix2 + NEXT_CHILD, prefix2 + NO_CHILD));

			return lines.Concat(Visit(statement.Condition, prefix2 + LAST_CHILD, prefix2 + SPACING));
		}
	}
}
