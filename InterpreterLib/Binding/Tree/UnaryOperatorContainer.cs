using InterpreterLib.Symbols.Types;
using System.Collections.Generic;

namespace InterpreterLib.Binding.Tree {
	internal sealed class UnaryOperatorContainer {

		private HashSet<UnaryOperator> operators;
		public IEnumerable<UnaryOperator> Operators => operators;

		public UnaryOperatorContainer() {
			operators = new HashSet<UnaryOperator>();

			AddUnaryOperator("+", UnaryOperatorType.Identity, ValueTypeSymbol.Integer, ValueTypeSymbol.Double, ValueTypeSymbol.Byte);
			AddUnaryOperator("-", UnaryOperatorType.Negation, ValueTypeSymbol.Integer, ValueTypeSymbol.Double, ValueTypeSymbol.Byte);
			AddUnaryOperator("!", UnaryOperatorType.LogicalNot, ValueTypeSymbol.Boolean);
		}

		private void AddUnaryOperator(string token, UnaryOperatorType type, params ValueTypeSymbol[] allowedTypes) {
			foreach(var allowedType in allowedTypes) {
				operators.Add(new UnaryOperator(token, type, allowedType));
			}
		}
	}
}
