using InterpreterLib.Binding;
using InterpreterLib.Binding.Tree;
using System;
using System.Collections.Generic;
using InterpreterLib.Binding.Types;

namespace InterpreterLib {
	internal class Evaluator : BoundTreeVisitor<object> {

		private DiagnosticContainer diagnostics;
		private BoundNode root;

		private Dictionary<VariableSymbol, object> variables;

		internal Evaluator(BoundNode rootNode, Dictionary<VariableSymbol, object> variables) {
			root = rootNode;
			diagnostics = new DiagnosticContainer();
			this.variables = variables;
		}

		public DiagnosticResult<object> Evaluate() {
			object value = null;

			try {
				value = Visit(root);
			} catch(ErrorEncounteredException exception) {  } // Allows the evaluator to exit if an error node is found.


			return new DiagnosticResult<object>(diagnostics, value);
		}

		protected override object VisitForStatement(BoundForStatement statement) {
			object assign = Visit(statement.Assignment);

			if (assign == null)
				return null;

			return Visit(statement.While);
		}

		protected override object VisitVariableDeclaration(BoundDeclarationStatement expression) {
			var variable = expression.VariableExpression.Variable;

			if (variables.ContainsKey(variable))
				variables[variable] = null;
			else
				variables.Add(variable, null);

			return null;
		}

		protected override object VisitWhile(BoundWhileStatement expression) {
			object outval = null;
			if (expression.Condition.ValueType != TypeSymbol.Boolean)
				throw new Exception("Invalid expr");

			while ((bool)Visit(expression.Condition)) {
				outval = Visit(expression.Body);
			}

			return outval;
		}

		protected override object VisitIf(BoundIfStatement ifStat) {
			if (ifStat.Condition.ValueType != TypeSymbol.Boolean)
				throw new Exception("Invalid expr");

			bool testCondition = (bool)Visit(ifStat.Condition);
			if (testCondition)
				return Visit(ifStat.TrueBranch);
			else if (ifStat.FalseBranch != null)
				return Visit(ifStat.FalseBranch);

			return null;
		}

		protected override object VisitBlock(BoundBlock expression) {
			object val = null;

			foreach (var stat in expression.Statements)
				val = Visit(stat);

			return val;
		}

		protected override object VisitAssignmentExpression(BoundAssignmentStatement assignment) {
			object expression = Visit(assignment.Expression);

			if (expression == null) 
				return null;

			variables[assignment.Identifier] = expression;
			return expression;
		}

		protected override object VisitVariable(BoundVariableExpression expression) {
			return variables[expression.Variable];
		}

		protected override object VisitBinaryExpression(BoundBinaryExpression expression) {
			object left = Visit(expression.LeftExpression);
			object right = Visit(expression.RightExpression);

			return GetOperatorEvaluator(expression.Op)(left, right);
		}

		protected override object VisitUnaryExpression(BoundUnaryExpression expression) {
			object operandValue = Visit(expression.Operand);

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

		private bool VisitEqualityOperation(object left, object right, BinaryOperator op) {
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
					return (left, right) => VisitEqualityOperation(left, right, op);
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

		protected override object VisitLiteral(BoundLiteral literal) {
			return literal.Value;
		}

		protected override object VisitError(BoundError error) {
			// If there is an error in the binding, we use an exception to bail out of the evaluation
			throw new ErrorEncounteredException();
		}
	}
}
