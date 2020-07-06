using InterpreterLib.Types;
using System.Collections.Generic;

namespace InterpreterLib.Binding.Tree {
	internal sealed class UnaryOperatorContainer {

		private HashSet<UnaryOperator> operators;
		public IEnumerable<UnaryOperator> Operators => operators;

		public UnaryOperatorContainer() {
			operators = new HashSet<UnaryOperator>();

			AddUnaryOperator("+", UnaryOperatorType.Identity, TypeSymbol.Integer, TypeSymbol.Double, TypeSymbol.Byte);
			AddUnaryOperator("-", UnaryOperatorType.Negation, TypeSymbol.Integer, TypeSymbol.Double, TypeSymbol.Byte);
			AddUnaryOperator("!", UnaryOperatorType.LogicalNot, TypeSymbol.Boolean);
		}

		private void AddUnaryOperator(string token, UnaryOperatorType type, params TypeSymbol[] allowedTypes) {
			foreach(var allowedType in allowedTypes) {
				operators.Add(new UnaryOperator(token, type, allowedType));
			}
		}
	}
}
