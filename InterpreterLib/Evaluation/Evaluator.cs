using InterpreterLib.Binding;
using InterpreterLib.Binding.Tree;
using System;
using System.Collections.Generic;
using InterpreterLib.Binding.Types;
using InterpreterLib.Binding.Tree.Statements;
using InterpreterLib.Binding.Tree.Expressions;

namespace InterpreterLib {
	internal class Evaluator {

		private DiagnosticContainer diagnostics;

		private Dictionary<VariableSymbol, object> variables;

		public BoundStatement[] Statements { get; }

		internal Evaluator(BoundStatement[] statements, Dictionary<VariableSymbol, object> variables) {
			diagnostics = new DiagnosticContainer();
			Statements = statements;
			this.variables = variables;
		}

		public DiagnosticResult<object> Evaluate() {
			object value = null;

			try {
				value = EvaluateStatements(Statements);
			} catch (ErrorEncounteredException exception) {
				diagnostics.AddDiagnostic(Diagnostic.ReportErrorEncounteredWhileEvaluating());
			} // Allows the evaluator to exit if an error node is found.


			return new DiagnosticResult<object>(diagnostics, value);
		}

		private object EvaluateStatements(BoundStatement[] statements) {
			var labelConversions = new Dictionary<LabelSymbol, int>();

			for (int index = 0; index < statements.Length; index++) {
				if (statements[index] is BoundLabel label) {
					labelConversions.Add(label.Label, index + 1);
				}
			}

			var currentIndex = 0;
			object val = null;
			while (currentIndex < statements.Length) {
				var statement = statements[currentIndex];

				switch (statement.Type) {
					case NodeType.VariableDeclaration:
						val = EvaluateVariableDeclaration((BoundVariableDeclarationStatement)statement);
						currentIndex++;
						break;
					case NodeType.Expression:
						val = EvaluateExpressionStatement((BoundExpressionStatement)statement);
						currentIndex++;
						break;
					case NodeType.ConditionalBranch:
						var cBranch = (BoundConditionalBranchStatement)statement;
						if ((bool)EvaluateExpression(cBranch.Condition) == cBranch.BranchIfTrue)
							currentIndex = labelConversions[cBranch.Label];
						else
							currentIndex++;

						break;
					case NodeType.Branch:
						var branch = (BoundBranchStatement)statement;
						currentIndex = labelConversions[branch.Label];
						break;
					case NodeType.Label:
						currentIndex++;
						break;

					default: throw new NotImplementedException();
				}
			}

			return val;
		}

		private object EvaluateExpression(BoundExpression expression) {
			switch (expression.Type) {
				case NodeType.Literal:
					return EvaluateLiteral((BoundLiteral)expression);
				case NodeType.Variable:
					return EvaluateVariable((BoundVariableExpression)expression);
				case NodeType.UnaryExpression:
					return EvaluateUnaryExpression((BoundUnaryExpression)expression);
				case NodeType.BinaryExpression:
					return EvaluateBinaryExpression((BoundBinaryExpression)expression);
				case NodeType.AssignmentExpression:
					return EvaluateAssignmentExpression((BoundAssignmentExpression)expression);

				default: throw new NotImplementedException();
			}
		}

		private object EvaluateVariableDeclaration(BoundVariableDeclarationStatement expression) {
			var exprVal = expression.Initialiser == null ? null : EvaluateExpression(expression.Initialiser);

			variables[expression.Variable] = exprVal;
			return exprVal;
		}

		private object EvaluateAssignmentExpression(BoundAssignmentExpression assignment) {
			object expression = EvaluateExpression(assignment.Expression);

			if (expression == null)
				return null;

			variables[assignment.Identifier] = expression;
			return expression;
		}

		private object EvaluateVariable(BoundVariableExpression expression) {
			return variables[expression.Variable];
		}

		private object EvaluateBinaryExpression(BoundBinaryExpression expression) {
			object left = EvaluateExpression(expression.LeftExpression);
			object right = EvaluateExpression(expression.RightExpression);

			return GetOperatorEvaluator(expression.Op)(left, right);
		}

		private object EvaluateUnaryExpression(BoundUnaryExpression expression) {
			object operandValue = EvaluateExpression(expression.Operand);

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
				case BinaryOperatorType.Concatonate:
					return (left, right) => (string)left + (string)right;
				default:
					throw new Exception("Invalid operator");
			}
		}

		private object EvaluateLiteral(BoundLiteral literal) {
			return literal.Value;
		}

		private object EvaluateError(BoundError error) {
			// If there is an error in the binding, we use an exception to bail out of the evaluation
			throw new ErrorEncounteredException();
		}

		private object EvaluateExpressionStatement(BoundExpressionStatement statement) {
			return EvaluateExpression(statement.Expression);
		}
	}
}
