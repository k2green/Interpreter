using InterpreterLib.Binding.Types;
using System;
using System.Collections.Generic;

namespace InterpreterLib.Binding.Tree {
	internal sealed class UnaryOperatorContainer {

		private HashSet<UnaryOperator> operators;
		public IEnumerable<UnaryOperator> Operators => operators;

		public UnaryOperatorContainer() {
			operators = new HashSet<UnaryOperator>();

			AddUnaryOperator("+", UnaryOperatorType.Identity, BoundType.Integer, BoundType.Double, BoundType.Byte);
			AddUnaryOperator("-", UnaryOperatorType.Negation, BoundType.Integer, BoundType.Double, BoundType.Byte);
			AddUnaryOperator("!", UnaryOperatorType.LogicalNot, BoundType.Boolean);
		}

		private void AddUnaryOperator(string token, UnaryOperatorType type, params BoundType[] allowedTypes) {
			foreach(var allowedType in allowedTypes) {
				operators.Add(new UnaryOperator(token, type, allowedType));
			}
		}
	}
}
