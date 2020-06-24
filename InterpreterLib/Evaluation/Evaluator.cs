using InterpreterLib.Binding;
using InterpreterLib.Binding.Tree;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Antlr4.Runtime.Tree;
using InterpreterLib.Binding.Types;

namespace InterpreterLib {
	public class Evaluator {
		private DiagnosticContainer diagnostics;
		private BoundNode root;

		private Dictionary<BoundVariable, object> variables;

		internal Evaluator(BoundNode rootNode, Dictionary<BoundVariable, object> variables) {
			root = rootNode;
			diagnostics = new DiagnosticContainer();
			this.variables = variables;
		}

		public DiagnosticResult<object> Evaluate() {
			object value = Evaluate(root);
			return new DiagnosticResult<object>(diagnostics, value);
		}

		private object Evaluate(BoundNode expression) {
			switch (expression.Type) {
				case NodeType.Literal:
					return EvaluateLiteral((BoundLiteral)expression);
				case NodeType.UnaryExpression:
					return EvaluateUnaryExpression((BoundUnaryExpression)expression);
				case NodeType.BinaryExpression:
					return EvaluateBinaryExpression((BoundBinaryExpression)expression);
				case NodeType.AssignmentExpression:
					return EvaluateAssignmentExpression((BoundAssignmentExpression)expression);
				case NodeType.Variable:
					return EvaluateVariable((BoundVariableExpression)expression);
				case NodeType.Expression:
					return EvaluateExpression((BoundExpressionStatement)expression);
				case NodeType.Block:
					return EvaluateBlock((BoundBlock)expression);
				case NodeType.If:
					return EvaluateIf((BoundIfStatement)expression);
				default: throw new Exception("Unimplemented node evaluator");
			}
		}

		private object EvaluateIf(BoundIfStatement ifStat) {
			if (ifStat.Condition.ValueType != BoundType.Boolean)
				throw new Exception("Invalid expr");

			bool testCondition = (bool)Evaluate(ifStat.Condition);
			if (testCondition)
				return Evaluate(ifStat.TrueBranch);
			else if (ifStat.FalseBranch != null)
				return Evaluate(ifStat.FalseBranch);

			return null;
		}

		private object EvaluateBlock(BoundBlock expression) {
			foreach (var stat in expression.Statements)
				Evaluate(stat);

			return null;
		}

		private object EvaluateExpression(BoundExpressionStatement expression) {
			return Evaluate(expression.Expression);
		}

		private object EvaluateAssignmentExpression(BoundAssignmentExpression assignment) {
			object expression = Evaluate(assignment.Expression);

			if (expression == null) {
				diagnostics.AddDiagnostic(new Diagnostic(0, 0, "Failed to evaluate expression"));
				return null;
			}

			variables[assignment.Identifier] = expression;
			return expression;
		}

		private object EvaluateVariable(BoundVariableExpression expression) {
			return variables[expression.Variable];
		}

		private object EvaluateBinaryExpression(BoundBinaryExpression expression) {
			object left = Evaluate(expression.LeftExpression);
			object right = Evaluate(expression.RightExpression);

			switch (expression.Op.OperatorType) {
				case BinaryOperatorType.Addition:
					return (int)left + (int)right;
				case BinaryOperatorType.Subtraction:
					return (int)left - (int)right;
				case BinaryOperatorType.Multiplication:
					return (int)left * (int)right;
				case BinaryOperatorType.Division:
					return (int)left / (int)right;
				case BinaryOperatorType.Power:
					return (int)Math.Pow((int)left, (int)right);
				case BinaryOperatorType.Modulus:
					return (int)left % (int)right;
				case BinaryOperatorType.Equality:
					return EvaluateEqualityOperation(left, right, expression.Op);
				case BinaryOperatorType.LogicalAnd:
					return (bool)left && (bool)right;
				case BinaryOperatorType.LogicalOr:
					return (bool)left || (bool)right;
				default: throw new Exception("Unimplemented binary operation");
			}
		}

		private object EvaluateUnaryExpression(BoundUnaryExpression expression) {
			object operandValue = Evaluate(expression.Operand);

			switch (expression.Op.OperatorType) {
				case UnaryOperatorType.Identity:
					return operandValue;
				case UnaryOperatorType.Negation:
					return -((int)operandValue);
				case UnaryOperatorType.LogicalNot:
					return !((bool)operandValue);
				default: throw new Exception("Unimplemented unary operation");
			}
		}

		private bool EvaluateEqualityOperation(object left, object right, BinaryOperator op) {
			if (op.LeftType == BoundType.Integer && op.RightType == BoundType.Integer)
				return ((int)left) == ((int)right);

			if (op.LeftType == BoundType.Boolean && op.RightType == BoundType.Boolean)
				return ((bool)left) == ((bool)right);

			return false;
		}

		private object EvaluateLiteral(BoundLiteral literal) {
			return literal.Value;
		}
	}
}
