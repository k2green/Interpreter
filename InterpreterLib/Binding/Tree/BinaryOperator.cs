using InterpreterLib.Binding.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Binding.Tree {
	internal sealed class BinaryOperator {

		public string TokenText { get; }
		public BinaryOperatorType OperatorType { get; }
		public BoundType LeftType;
		public BoundType RightType;
		public BoundType OutputType;

		public BinaryOperator(string tokenText, BinaryOperatorType operatorType, BoundType inputType, BoundType outputType) : this(tokenText, operatorType, inputType, inputType, outputType) { }

		public BinaryOperator(string tokenText, BinaryOperatorType operatorType, BoundType leftType, BoundType rightType, BoundType outputType) {
			TokenText = tokenText;
			OperatorType = operatorType;
			LeftType = leftType;
			RightType = rightType;
			OutputType = outputType;
		}

		private static BinaryOperator[] operators = {
			new BinaryOperator("+", BinaryOperatorType.Addition, BoundType.Integer, BoundType.Integer),
			new BinaryOperator("-", BinaryOperatorType.Subtraction, BoundType.Integer, BoundType.Integer),
			new BinaryOperator("*", BinaryOperatorType.Multiplication, BoundType.Integer, BoundType.Integer),
			new BinaryOperator("/", BinaryOperatorType.Division, BoundType.Integer, BoundType.Integer),
			new BinaryOperator("^", BinaryOperatorType.Power, BoundType.Integer, BoundType.Integer),
			new BinaryOperator("mod", BinaryOperatorType.Modulus, BoundType.Integer, BoundType.Integer),

			new BinaryOperator("==", BinaryOperatorType.Equality, BoundType.Integer, BoundType.Boolean),
			new BinaryOperator("==", BinaryOperatorType.Equality, BoundType.Boolean, BoundType.Boolean),

			new BinaryOperator("&&", BinaryOperatorType.LogicalAnd, BoundType.Boolean, BoundType.Boolean),
			new BinaryOperator("||", BinaryOperatorType.LogicalOr, BoundType.Boolean, BoundType.Boolean)
		};

		public static BinaryOperator Bind(string opText, BoundType leftType, BoundType rightType) {
			foreach (BinaryOperator op in operators) {
				if (op.TokenText.Equals(opText) && op.LeftType == leftType && op.RightType == rightType)
					return op;
			}

			return null;
		}
	}
}
