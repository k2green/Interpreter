using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Binding.Tree {
	internal sealed class BinaryOperator {

		public string TokenText { get; }
		public BinaryOperatorType OperatorType { get; }
		public Type LeftType;
		public Type RightType;
		public Type OutputType;

		public BinaryOperator(string tokenText, BinaryOperatorType operatorType, Type inputType, Type outputType) : this(tokenText, operatorType, inputType, inputType, outputType) { }

		public BinaryOperator(string tokenText, BinaryOperatorType operatorType, Type leftType, Type rightType, Type outputType) {
			TokenText = tokenText;
			OperatorType = operatorType;
			LeftType = leftType;
			RightType = rightType;
			OutputType = outputType;
		}

		private static BinaryOperator[] operators = {
			new BinaryOperator("+", BinaryOperatorType.Addition, typeof(int), typeof(int)),
			new BinaryOperator("-", BinaryOperatorType.Subtraction, typeof(int), typeof(int)),
			new BinaryOperator("*", BinaryOperatorType.Multiplication, typeof(int), typeof(int)),
			new BinaryOperator("/", BinaryOperatorType.Division, typeof(int), typeof(int)),
			new BinaryOperator("^", BinaryOperatorType.Power, typeof(int), typeof(int)),
			new BinaryOperator("mod", BinaryOperatorType.Modulus, typeof(int), typeof(int)),

			new BinaryOperator("==", BinaryOperatorType.Equality, typeof(int), typeof(bool)),
			new BinaryOperator("==", BinaryOperatorType.Equality, typeof(bool), typeof(bool)),

			new BinaryOperator("&&", BinaryOperatorType.LogicalAnd, typeof(bool), typeof(bool)),
			new BinaryOperator("||", BinaryOperatorType.LogicalOr, typeof(bool), typeof(bool))
		};
		
		public static BinaryOperator Bind(string opText, Type leftType, Type rightType) {
			foreach(BinaryOperator op in operators) {
				if (op.TokenText.Equals(opText) && op.LeftType == leftType && op.RightType == rightType)
					return op;
			}

			return null;
		}
	}
}
