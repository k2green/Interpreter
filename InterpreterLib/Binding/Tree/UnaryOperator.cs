using InterpreterLib.Binding.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Binding.Tree {
	internal sealed class UnaryOperator {

		public string TokenText { get; }
		public UnaryOperatorType OperatorType { get; }
		public BoundType OperandType;
		public BoundType OutputType;

		public UnaryOperator(string tokenText, UnaryOperatorType operatorType, BoundType operandType) : this(tokenText, operatorType, operandType, operandType) { }

		public UnaryOperator(string tokenText, UnaryOperatorType operatorType, BoundType operandType, BoundType outputType) {
			TokenText = tokenText;
			OperatorType = operatorType;
			OperandType = operandType;
			OutputType = outputType;
		}

		private static UnaryOperator[] operators = {
			new UnaryOperator("+", UnaryOperatorType.Identity, BoundType.Integer),
			new UnaryOperator("-", UnaryOperatorType.Negation, BoundType.Integer),
			new UnaryOperator("+", UnaryOperatorType.Identity, BoundType.Double),
			new UnaryOperator("-", UnaryOperatorType.Negation, BoundType.Double),
			new UnaryOperator("!", UnaryOperatorType.LogicalNot, BoundType.Integer),
		};

		public static UnaryOperator Bind(string opText, BoundType inputType) {
			foreach(UnaryOperator op in operators) {
				if (op.TokenText.Equals(opText) && op.OperandType.Equals(inputType))
					return op;
			}

			return null;
		}
	}
}
