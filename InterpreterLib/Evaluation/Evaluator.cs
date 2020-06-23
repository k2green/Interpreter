using InterpreterLib.Binding;
using InterpreterLib.Binding.Tree;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Antlr4.Runtime.Tree;

namespace InterpreterLib {
	public class Evaluator {
		private DiagnosticContainer diagnostics;
		private BoundExpression root;

		private Dictionary<BoundVariable, object> variables;

		internal Evaluator(BoundExpression rootNode, Dictionary<BoundVariable, object> variables) {
			root = rootNode;
			diagnostics = new DiagnosticContainer();
			this.variables = variables;
		}

		public DiagnosticResult<object> Evaluate() {
			object value = Evaluate(root);
			return new DiagnosticResult<object>(diagnostics, value);
		}

		private object Evaluate(BoundExpression expression) {
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
				default: throw new Exception("Unimplemented node evaluator");
			}
		}

		private object EvaluateAssignmentExpression(BoundAssignmentExpression assignment) {
			object expression = Evaluate(assignment.Expression);
			variables[assignment.Identifier] = expression ?? throw new Exception("Unable to evaluate assignment");
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
			if (op.LeftType == typeof(int) && op.RightType == typeof(int))
				return ((int)left) == ((int)right);

			if (op.LeftType == typeof(bool) && op.RightType == typeof(bool))
				return ((bool)left) == ((bool)right);

			return false;
		}

		private object EvaluateLiteral(BoundLiteral literal) {
			return literal.Value;
		}
	}
}
