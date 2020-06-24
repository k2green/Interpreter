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

		private static IEnumerable<BinaryOperator> operators;
		private static IEnumerable<BinaryOperator> Operators { get {
				if (operators == null)
					operators = new BinaryOperatorContainor().Operators;

				return operators;
			} }

		public static BinaryOperator Bind(string opText, BoundType leftType, BoundType rightType) {
			foreach (BinaryOperator op in Operators) {
				if (op.TokenText.Equals(opText) && op.LeftType == leftType && op.RightType == rightType)
					return op;
			}

			return null;
		}
	}
}
