using InterpreterLib.Binding.Tree;
using InterpreterLib.Binding.Tree.Expressions;
using InterpreterLib.Binding.Tree.Statements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InterpreterLib.Diagnostics {
	internal class BoundTreeDisplayVisitor {
		public const string NEXT_CHILD = "  ├─";
		public const string NO_CHILD = "  │ ";
		public const string LAST_CHILD = "  └─";
		public const string SPACING = "    ";


		public const ConsoleColor DEFAULT_COLOR = ConsoleColor.White;
		public const ConsoleColor STATEMENT_COLOR = ConsoleColor.DarkYellow;
		public const ConsoleColor EXPRESSION_COLOR = ConsoleColor.Cyan;
		public const ConsoleColor LITERAL_COLOR = ConsoleColor.Green;
		public const ConsoleColor VARAIBLE_COLOR = ConsoleColor.Cyan;
		public const ConsoleColor LABEL_COLOR = ConsoleColor.Gray;
		public const ConsoleColor BRANCH_COLOR = ConsoleColor.DarkMagenta;
		public const ConsoleColor CONVERSION_COLOR = ConsoleColor.DarkCyan;
		public const ConsoleColor FUNCTION = ConsoleColor.DarkCyan;

		public void GetText(BoundStatement[] statements) {
			Console.ForegroundColor = DEFAULT_COLOR;
			Console.Write("└─");

			Console.ForegroundColor = STATEMENT_COLOR;
			Console.WriteLine("Block: ");

			string prefix1 = "  " + NEXT_CHILD;
			string prefix2 = "  " + NO_CHILD;

			string LastPrefix1 = "  " + LAST_CHILD;
			string LastPrefix2 = "  " + SPACING;

			for (int i = 0; i < statements.Length - 1; i++)
				Visit(statements[i], prefix1, prefix2);

			Visit(statements[statements.Length - 1], LastPrefix1, LastPrefix2);
		}

		public void GetText(BoundNode node) {
			Visit(node, "", "");
		}

		protected void Visit(BoundNode node, string prefix1, string prefix2) {
			Console.ForegroundColor = DEFAULT_COLOR;

			switch (node.Type) {
				case NodeType.Literal:
					VisitLiteral((BoundLiteral)node, prefix1, prefix2); break;
				case NodeType.UnaryExpression:
					VisitUnaryExpression((BoundUnaryExpression)node, prefix1, prefix2); break;
				case NodeType.BinaryExpression:
					VisitBinaryExpression((BoundBinaryExpression)node, prefix1, prefix2); break;
				case NodeType.AssignmentExpression:
					VisitAssignmentExpression((BoundAssignmentExpression)node, prefix1, prefix2); break;
				case NodeType.Variable:
					VisitVariableExpression((BoundVariableExpression)node, prefix1, prefix2); break;
				case NodeType.Block:
					VisitBlock((BoundBlock)node, prefix1, prefix2); break;
				case NodeType.If:
					VisitIfStatement((BoundIfStatement)node, prefix1, prefix2); break;
				case NodeType.While:
					VisitWhileStatement((BoundWhileStatement)node, prefix1, prefix2); break;
				case NodeType.VariableDeclaration:
					VisitVariableDeclaration((BoundVariableDeclarationStatement)node, prefix1, prefix2); break;
				case NodeType.For:
					VisitForStatement((BoundForStatement)node, prefix1, prefix2); break;
				case NodeType.Error:
					VisitError((BoundError)node, prefix1, prefix2); break;
				case NodeType.Expression:
					VisitExpressionStatement((BoundExpressionStatement)node, prefix1, prefix2); break;
				case NodeType.ConditionalBranch:
					VisitConditionalBranchStatement((BoundConditionalBranchStatement)node, prefix1, prefix2); break;
				case NodeType.Branch:
					VisitBranchStatement((BoundBranchStatement)node, prefix1, prefix2); break;
				case NodeType.Label:
					VisitLabelStatement((BoundLabel)node, prefix1, prefix2); break;
				case NodeType.InternalTypeConversion:
					VisitLabelInternalTypeConversion((BoundInternalTypeConversion)node, prefix1, prefix2); break;
				case NodeType.FunctionCall:
					VisitFunctionCall((BoundFunctionCall)node, prefix1, prefix2); break;
				default: throw new Exception("Unimplemented node evaluator");
			}
		}

		private void VisitFunctionCall(BoundFunctionCall node, string prefix1, string prefix2) {
			Console.Write(prefix1);

			Console.ForegroundColor = FUNCTION;
			Console.WriteLine($"Function call: {node.Function.Name} => {node.Function.ReturnType}");

			List<BoundExpression> parameters = new List<BoundExpression>(node.Parameters);
			if (parameters.Count > 0) {
				for (int index = 0; index < parameters.Count - 1; index++)
					Visit(parameters[index], prefix2 + NEXT_CHILD, prefix2 + NO_CHILD);

				Visit(parameters[parameters.Count - 1], prefix2 + LAST_CHILD, prefix2 + SPACING);
			}
		}

		private void VisitLabelInternalTypeConversion(BoundInternalTypeConversion node, string prefix1, string prefix2) {
			Console.Write(prefix1);

			Console.ForegroundColor = CONVERSION_COLOR;
			Console.WriteLine($"Type Converstion: {node.ConversionSymbol}");

			Visit(node.Expression, prefix2 + LAST_CHILD, prefix2 + SPACING);
		}

		private void VisitAssignmentExpression(BoundAssignmentExpression expression, string prefix1, string prefix2) {
			Console.Write(prefix1);

			Console.ForegroundColor = EXPRESSION_COLOR;
			Console.WriteLine($"Assignment Expression: {expression.Identifier}");

			Console.ForegroundColor = DEFAULT_COLOR;
			Console.Write($"{prefix2}{NEXT_CHILD}");

			Console.ForegroundColor = EXPRESSION_COLOR;
			Console.WriteLine($"Variable: {expression.Identifier}");

			Visit(expression.Expression, prefix2 + LAST_CHILD, prefix2 + SPACING);
		}

		private void VisitBinaryExpression(BoundBinaryExpression expression, string prefix1, string prefix2) {
			var op = expression.Op;

			Console.Write(prefix1);
			Console.ForegroundColor = EXPRESSION_COLOR;

			Console.WriteLine($"BinaryExpression");
			Visit(expression.LeftExpression, prefix2 + NEXT_CHILD, prefix2 + NO_CHILD);

			Console.ForegroundColor = DEFAULT_COLOR;
			Console.Write($"{prefix2}{NEXT_CHILD}");

			Console.ForegroundColor = EXPRESSION_COLOR;
			Console.WriteLine($"{op.TokenText} : ({op.LeftType}, {op.RightType}) => {op.OutputType}");

			Visit(expression.RightExpression, prefix2 + LAST_CHILD, prefix2 + SPACING);
		}

		private void VisitBlock(BoundBlock block, string prefix1, string prefix2) {
			Console.Write(prefix1);

			Console.ForegroundColor = STATEMENT_COLOR;
			Console.WriteLine($"Block");

			if (block.Statements != null && block.Statements.Length > 0) {
				for (int i = 0; i < block.Statements.Length - 1; i++)
					Visit(block.Statements[i], prefix2 + NEXT_CHILD, prefix2 + NO_CHILD);

				Visit(block.Statements[block.Statements.Length - 1], prefix2 + LAST_CHILD, prefix2 + SPACING);
			}
		}

		private void VisitError(BoundError error, string prefix1, string prefix2) {
			Console.WriteLine($"{prefix1}{error.Error}");
		}

		private void VisitExpressionStatement(BoundExpressionStatement statement, string prefix1, string prefix2) {
			Console.Write(prefix1);

			Console.ForegroundColor = STATEMENT_COLOR;
			Console.WriteLine($"Expression Statement");

			Visit(statement.Expression, prefix2 + LAST_CHILD, prefix2 + SPACING);
		}

		private void VisitForStatement(BoundForStatement statement, string prefix1, string prefix2) {
			Console.Write(prefix1);

			Console.ForegroundColor = STATEMENT_COLOR;
			Console.WriteLine($"For Statement");

			Visit(statement.Assignment, prefix2 + NEXT_CHILD + "a: ", prefix2 + NO_CHILD);
			Visit(statement.Condition, prefix2 + NEXT_CHILD + "c: ", prefix2 + NO_CHILD);
			Visit(statement.Step, prefix2 + NEXT_CHILD + "s:", prefix2 + NO_CHILD);
			Visit(statement.Body, prefix2 + LAST_CHILD, prefix2 + SPACING);
		}

		private void VisitIfStatement(BoundIfStatement statement, string prefix1, string prefix2) {
			Console.Write(prefix1);

			Console.ForegroundColor = STATEMENT_COLOR;
			Console.WriteLine($"For Statement");

			Visit(statement.Condition, prefix2 + NEXT_CHILD, prefix2 + NO_CHILD);

			if (statement.FalseBranch != null) {
				Visit(statement.TrueBranch, prefix2 + NEXT_CHILD, prefix2 + NO_CHILD);
				Visit(statement.FalseBranch, prefix2 + LAST_CHILD, prefix2 + SPACING);
			} else {
				Visit(statement.TrueBranch, prefix2 + LAST_CHILD, prefix2 + SPACING);
			}
		}

		private void VisitLiteral(BoundLiteral literal, string prefix1, string prefix2) {
			Console.Write(prefix1);

			Console.ForegroundColor = LITERAL_COLOR;
			Console.WriteLine($"Literal = {literal.Value} : {literal.ValueType}");
		}

		private void VisitUnaryExpression(BoundUnaryExpression expression, string prefix1, string prefix2) {
			var op = expression.Op;
			Console.Write(prefix1);

			Console.ForegroundColor = EXPRESSION_COLOR;
			Console.WriteLine($"Unary expression");

			Console.ForegroundColor = DEFAULT_COLOR;
			Console.Write($"{prefix2}{NEXT_CHILD}");

			Console.ForegroundColor = EXPRESSION_COLOR;
			Console.WriteLine($"{op.TokenText} : {op.OperandType} => {op.OutputType}");

			Visit(expression.Operand, prefix2 + LAST_CHILD, prefix2 + SPACING);
		}

		private void VisitVariableExpression(BoundVariableExpression expression, string prefix1, string prefix2) {
			Console.Write(prefix1);

			Console.ForegroundColor = VARAIBLE_COLOR;
			Console.WriteLine($"Variable { expression.Variable}");
		}

		private void VisitVariableDeclaration(BoundVariableDeclarationStatement statement, string prefix1, string prefix2) {
			Console.Write(prefix1);

			Console.ForegroundColor = STATEMENT_COLOR;
			Console.WriteLine("Variable Declaration");

			Console.ForegroundColor = DEFAULT_COLOR;
			Console.Write($"{prefix2}{LAST_CHILD}");

			Console.ForegroundColor = STATEMENT_COLOR;
			Console.WriteLine($"Variable Name: {statement.Variable}");

			if (statement.Initialiser != null)
				Visit(statement.Initialiser, prefix2 + LAST_CHILD, prefix2 + SPACING);
		}

		private void VisitWhileStatement(BoundWhileStatement statement, string prefix1, string prefix2) {
			Console.Write(prefix1);

			Console.ForegroundColor = STATEMENT_COLOR;
			Console.WriteLine($"While Statement");

			Visit(statement.Condition, prefix2 + NEXT_CHILD, prefix2 + NO_CHILD);
			Visit(statement.Body, prefix2 + LAST_CHILD, prefix2 + SPACING);
		}

		private void VisitConditionalBranchStatement(BoundConditionalBranchStatement statement, string prefix1, string prefix2) {
			Console.Write(prefix1);

			Console.ForegroundColor = BRANCH_COLOR;
			Console.WriteLine($"Conditional branch statement");

			Visit(statement.Condition, prefix2 + NEXT_CHILD, prefix2 + NO_CHILD);

			Console.ForegroundColor = DEFAULT_COLOR;
			Console.Write($"{prefix2}{NEXT_CHILD}");

			Console.ForegroundColor = BRANCH_COLOR;
			Console.WriteLine($"Branch if true = {statement.BranchIfTrue}");

			Console.ForegroundColor = DEFAULT_COLOR;
			Console.Write($"{prefix2}{LAST_CHILD}");

			Console.ForegroundColor = BRANCH_COLOR;
			Console.WriteLine($"Label: {statement.Label}");
		}

		private void VisitBranchStatement(BoundBranchStatement statement, string prefix1, string prefix2) {
			Console.Write(prefix1);

			Console.ForegroundColor = BRANCH_COLOR;
			Console.WriteLine($"Branch statement");

			Console.ForegroundColor = DEFAULT_COLOR;
			Console.Write($"{prefix2}{LAST_CHILD}");

			Console.ForegroundColor = BRANCH_COLOR;
			Console.WriteLine($"Label: {statement.Label}");
		}

		private void VisitLabelStatement(BoundLabel statement, string prefix1, string prefix2) {
			Console.Write(prefix1);

			Console.ForegroundColor = LABEL_COLOR;
			Console.WriteLine($"Label: {statement.Label}");
		}
	}
}
