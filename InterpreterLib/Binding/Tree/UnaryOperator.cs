using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Binding.Tree {
	internal sealed class UnaryOperator {

		public string TokenText { get; }
		public UnaryOperatorType OperatorType { get; }
		public Type OperandType;
		public Type OutputType;

		public UnaryOperator(string tokenText, UnaryOperatorType operatorType, Type operandType) : this(tokenText, operatorType, operandType, operandType) { }

		public UnaryOperator(string tokenText, UnaryOperatorType operatorType, Type operandType, Type outputType) {
			TokenText = tokenText;
			OperatorType = operatorType;
			OperandType = operandType;
			OutputType = outputType;
		}

		private static UnaryOperator[] operators = {
			new UnaryOperator("+", UnaryOperatorType.Identity, typeof(int)),
			new UnaryOperator("-", UnaryOperatorType.Negation, typeof(int)),
			new UnaryOperator("!", UnaryOperatorType.LogicalNot, typeof(bool)),
		};

		public static UnaryOperator Bind(string opText, Type inputType) {
			foreach(UnaryOperator op in operators) {
				if (op.TokenText.Equals(opText) && op.OperandType == inputType)
					return op;
			}

			return null;
		}
	}
}
