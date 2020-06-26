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

		private Dictionary<VariableSymbol, object> variables;

		internal Evaluator(BoundNode rootNode, Dictionary<VariableSymbol, object> variables) {
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
				case NodeType.While:
					return EvaluateWhile((BoundWhileStatement)expression);
				case NodeType.VariableDeclaration:
					return EvaluateVariableDeclaration((BoundDeclarationStatement)expression);
				case NodeType.For:
					return EvaluateForStatement((BoundForStatement)expression);
				default: throw new Exception("Unimplemented node evaluator");
			}
		}

		private object EvaluateForStatement(BoundForStatement statement) {
			object assign = Evaluate(statement.Assignment);

			if (assign == null)
				return null;

			return Evaluate(statement.While);
		}

		private object EvaluateVariableDeclaration(BoundDeclarationStatement expression) {
			var variable = expression.VariableExpression.Variable;

			if (variables.ContainsKey(variable))
				variables[variable] = null;
			else
				variables.Add(variable, null);

			return null;
		}

		private object EvaluateWhile(BoundWhileStatement expression) {
			object outval = null;
			if (expression.Condition.ValueType != TypeSymbol.Boolean)
				throw new Exception("Invalid expr");

			while ((bool)Evaluate(expression.Condition)) {
				outval = Evaluate(expression.Body);
			}

			return outval;
		}

		private object EvaluateIf(BoundIfStatement ifStat) {
			if (ifStat.Condition.ValueType != TypeSymbol.Boolean)
				throw new Exception("Invalid expr");

			bool testCondition = (bool)Evaluate(ifStat.Condition);
			if (testCondition)
				return Evaluate(ifStat.TrueBranch);
			else if (ifStat.FalseBranch != null)
				return Evaluate(ifStat.FalseBranch);

			return null;
		}

		private object EvaluateBlock(BoundBlock expression) {
			object val = null;

			foreach (var stat in expression.Statements)
				val = Evaluate(stat);

			return val;
		}

		private object EvaluateExpression(BoundExpressionStatement expression) {
			return Evaluate(expression.Expression);
		}

		private object EvaluateAssignmentExpression(BoundAssignmentExpression assignment) {
			object expression = Evaluate(assignment.Expression);

			if (expression == null) 
				return null;

			variables[assignment.Identifier] = expression;
			return expression;
		}

		private object EvaluateVariable(BoundVariableExpression expression) {
			return variables[expression.Variable];
		}

		private object EvaluateBinaryExpression(BoundBinaryExpression expression) {
			object left = Evaluate(expression.LeftExpression);
			object right = Evaluate(expression.RightExpression);

			return GetOperatorEvaluator(expression.Op)(left, right);
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
			if (op.LeftType == TypeSymbol.Integer && op.RightType == TypeSymbol.Integer)
				return ((int)left) == ((int)right);

			if (op.LeftType == TypeSymbol.Boolean && op.RightType == TypeSymbol.Boolean)
				return ((bool)left) == ((bool)right);

			return false;
		}

		private Func<object, object, object> GetOperatorEvaluator(BinaryOperator op) {
			switch (op.OperatorType) {
				case BinaryOperatorType.Addition:
					return (left, right) => (int)left + (int)right;
				case BinaryOperatorType.Subtraction:
					return (left, right) => (int)left - (int)right;
				case BinaryOperatorType.Multiplication:
					return (left, right) => (int)left * (int)right;
				case BinaryOperatorType.Division:
					return (left, right) => (int)left / (int)right;
				case BinaryOperatorType.Power:
					return (left, right) => (int)Math.Pow((int)left, (int)right);
				case BinaryOperatorType.Modulus:
					return (left, right) => (int)left % (int)right;
				case BinaryOperatorType.Equality:
					return (left, right) => EvaluateEqualityOperation(left, right, op);
				case BinaryOperatorType.LogicalAnd:
					return (left, right) => (bool)left && (bool)right;
				case BinaryOperatorType.LogicalOr:
					return (left, right) => (bool)left || (bool)right;
				case BinaryOperatorType.LogicalXOr:
					return (left, right) => (bool)left ^ (bool)right;
				case BinaryOperatorType.GreaterThan:
					return (left, right) => (int)left >= (int)right; 
				case BinaryOperatorType.LesserThan:
					return (left, right) => (int)left <= (int)right;
				case BinaryOperatorType.StrictGreaterThan:
					return (left, right) => (int)left > (int)right;
				case BinaryOperatorType.StrinLesserThan:
					return (left, right) => (int)left < (int)right;
				default:
					throw new Exception("Invalid operator");
			}
		}

		private object EvaluateLiteral(BoundLiteral literal) {
			return literal.Value;
		}
	}
}
