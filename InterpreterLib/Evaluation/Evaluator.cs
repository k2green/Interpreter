using InterpreterLib.Binding;
using InterpreterLib.Binding.Tree;
using System;
using System.Collections.Generic;
using InterpreterLib.Binding.Tree.Statements;
using InterpreterLib.Binding.Tree.Expressions;
using InterpreterLib.Symbols.Binding;
using InterpreterLib.Symbols.Types;

namespace InterpreterLib.Diagnostics {
	internal class Evaluator {

		private DiagnosticContainer diagnostics;

		private Dictionary<VariableSymbol, object> globals;
		private Stack<Dictionary<VariableSymbol, object>> locals;

		public BoundProgram Program { get; }

		internal Evaluator(BoundProgram program, Dictionary<VariableSymbol, object> variables) {
			diagnostics = new DiagnosticContainer();
			Program = program;
			globals = variables;
			locals = new Stack<Dictionary<VariableSymbol, object>>();
		}

		public DiagnosticResult<object> Evaluate() {
			object value = null;

			try {
				value = EvaluateBlock(Program.Statement);
			} catch (ErrorEncounteredException exception) {
				diagnostics.AddDiagnostic(Diagnostic.ReportErrorEncounteredWhileEvaluating());
			} // Allows the evaluator to exit if an error node is found.


			return new DiagnosticResult<object>(diagnostics, value);
		}

		private object EvaluateBlock(BoundBlock block) {
			var labelConversions = new Dictionary<LabelSymbol, int>();

			for (int index = 0; index < block.Statements.Length; index++) {
				if (block.Statements[index] is BoundLabel label) {
					labelConversions.Add(label.Label, index + 1);
				}
			}

			var currentIndex = 0;
			object ret = null;
			while (currentIndex < block.Statements.Length) {
				var statement = block.Statements[currentIndex];
				object val = null;

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

				if (val != null)
					ret = val;
			}

			return ret;
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
				case NodeType.FunctionCall:
					return EvaluateFunctionCall((BoundFunctionCall)expression);
				case NodeType.InternalTypeConversion:
					return EvaluateInternalTypeConversion((BoundInternalTypeConversion)expression);
				default: throw new NotImplementedException();
			}
		}

		private object EvaluateFunctionCall(BoundFunctionCall statement) {
			if (statement.Function == BuiltInFunctions.Print) {
				string text = (string)EvaluateExpression(statement.Parameters[0]);
				Console.WriteLine(text);
			} else if (statement.Function == BuiltInFunctions.Input) {
				string input = Console.ReadLine();
				return input;
			} else if (Program.FunctionBodies.TryGetValue(statement.Function, out var boundBlock)) {
				var newScope = new Dictionary<VariableSymbol, object>();
				var parameters = new VariableSymbol[statement.Parameters.Length];
				int index = 0;

				foreach (var param in statement.Function.Parameters) {
					parameters[index] = new VariableSymbol(param.Name, false, param.ValueType);
					newScope.Add(parameters[index], null);
					index++;
				}

				for (index = 0; index < parameters.Length; index++) {
					newScope[parameters[index]] = EvaluateExpression(statement.Parameters[index]);
				}

				locals.Push(newScope);

				var value = EvaluateBlock(boundBlock);

				locals.Pop();

				return value;
			}

			return null;
		}

		private object EvaluateInternalTypeConversion(BoundInternalTypeConversion expression) {
			if (expression.ConversionSymbol.ToType == ValueTypeSymbol.String) {
				return EvaluateExpression(expression.Expression).ToString();
			}

			if (expression.ConversionSymbol.ToType == ValueTypeSymbol.Double)
				return Convert.ToDouble(EvaluateExpression(expression.Expression));

			if (expression.ConversionSymbol.ToType == ValueTypeSymbol.Integer)
				return Convert.ToInt32(EvaluateExpression(expression.Expression));

			throw new Exception($"Unhandled type conversion {expression.ConversionSymbol}");
		}

		private object EvaluateVariableDeclaration(BoundVariableDeclarationStatement expression) {
			var exprVal = expression.Initialiser == null ? null : EvaluateExpression(expression.Initialiser);

			Assign(expression.Variable, exprVal);
			return exprVal;
		}

		private object EvaluateAssignmentExpression(BoundAssignmentExpression assignment) {
			object expression = EvaluateExpression(assignment.Expression);

			if (expression == null)
				return null;

			Assign(assignment.Identifier, expression);
			return expression;
		}

		private object EvaluateVariable(BoundVariableExpression expression) {
			if (locals.Count > 0) {
				if (locals.Peek().TryGetValue(expression.Variable, out var value))
					return value;
			}

			return globals[expression.Variable];
		}

		private void Assign(VariableSymbol symbol, object value) {
			if (locals.Count > 0) {
				var localVars = locals.Peek();

				if (localVars.ContainsKey(symbol))
					localVars[symbol] = value;
				else
					localVars.Add(symbol, value);

				return;
			}

			if (globals.ContainsKey(symbol))
				globals[symbol] = value;
			else
				globals.Add(symbol, value);
		}

		private object EvaluateBinaryExpression(BoundBinaryExpression expression) {
			object left = EvaluateExpression(expression.LeftExpression);
			object right = EvaluateExpression(expression.RightExpression);
			var op = expression.Op;

			switch (op.OperatorType) {
				case BinaryOperatorType.Addition:
					return OperatorEvaluator.EvaluateAddition(left, right, op);
				case BinaryOperatorType.Subtraction:
					return OperatorEvaluator.EvaluateSubtraction(left, right, op);
				case BinaryOperatorType.Multiplication:
					return OperatorEvaluator.EvaluateMultiplication(left, right, op);
				case BinaryOperatorType.Division:
					return OperatorEvaluator.EvaluateDivision(left, right, op);
				case BinaryOperatorType.Power:
					return OperatorEvaluator.EvaluatePower(left, right, op);
				case BinaryOperatorType.Modulus:
					return (int)left % (int)right;
				case BinaryOperatorType.Equality:
					return OperatorEvaluator.EvaluateEquality(left, right, op);
				case BinaryOperatorType.NegatveEquality:
					return !OperatorEvaluator.EvaluateEquality(left, right, op);
				case BinaryOperatorType.LogicalAnd:
					return (bool)left && (bool)right;
				case BinaryOperatorType.LogicalOr:
					return (bool)left || (bool)right;
				case BinaryOperatorType.LogicalXOr:
					return (bool)left ^ (bool)right;
				case BinaryOperatorType.GreaterThan:
					return OperatorEvaluator.EvaluateGreaterThan(left, right, op);
				case BinaryOperatorType.LesserThan:
					return OperatorEvaluator.EvaluateLesserThan(left, right, op);
				case BinaryOperatorType.StrictGreaterThan:
					return OperatorEvaluator.EvaluateStrictGreaterThan(left, right, op);
				case BinaryOperatorType.StrinLesserThan:
					return OperatorEvaluator.EvaluateStrictLesserThan(left, right, op);
				case BinaryOperatorType.Concatonate:
					return OperatorEvaluator.EvaluateConcatonation(left, right);
				default:
					throw new Exception("Invalid operator");
			}
		}

		private object EvaluateUnaryExpression(BoundUnaryExpression expression) {
			object operandValue = EvaluateExpression(expression.Operand);

			switch (expression.Op.OperatorType) {
				case UnaryOperatorType.Identity:
					return operandValue;
				case UnaryOperatorType.LogicalNot:
					return !((bool)operandValue);
				case UnaryOperatorType.Negation:
					if (expression.Op.OutputType == ValueTypeSymbol.Integer)
						return -((int)operandValue);

					if (expression.Op.OutputType == ValueTypeSymbol.Double)
						return -((double)operandValue);

					if (expression.Op.OutputType == ValueTypeSymbol.Byte)
						return -((byte)operandValue);
					throw new Exception("Unimplemented unary operation");
				default:
					throw new Exception("Unimplemented unary operation");
			}
		}

		private object EvaluateLiteral(BoundLiteral literal) {
			return literal.Value;
		}

		private object EvaluateExpressionStatement(BoundExpressionStatement statement) {
			return EvaluateExpression(statement.Expression);
		}
	}
}
