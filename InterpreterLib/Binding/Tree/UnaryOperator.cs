using InterpreterLib.Symbols.Types;
using System.Collections.Generic;

namespace InterpreterLib.Binding.Tree {
	internal sealed class UnaryOperator {

		public string TokenText { get; }
		public UnaryOperatorType OperatorType { get; }
		public ValueTypeSymbol OperandType;
		public ValueTypeSymbol OutputType;

		public UnaryOperator(string tokenText, UnaryOperatorType operatorType, ValueTypeSymbol operandType) : this(tokenText, operatorType, operandType, operandType) { }

		public UnaryOperator(string tokenText, UnaryOperatorType operatorType, ValueTypeSymbol operandType, ValueTypeSymbol outputType) {
			TokenText = tokenText;
			OperatorType = operatorType;
			OperandType = operandType;
			OutputType = outputType;
		}

		private static IEnumerable<UnaryOperator> operators;
		private static IEnumerable<UnaryOperator> Operators {
			get {
				if (operators == null)
					operators = new UnaryOperatorContainer().Operators;

				return operators;
			}
		}

		public static UnaryOperator Bind(string opText, ValueTypeSymbol inputType) {
			foreach (UnaryOperator op in Operators) {
				if (op.TokenText.Equals(opText) && op.OperandType.Equals(inputType))
					return op;
			}

			return null;
		}

		public override string ToString() => TokenText;
	}
}
