using InterpreterLib.Binding.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Binding.Tree {
	internal sealed class BinaryOperatorContainor {
		private HashSet<BinaryOperator> operators;
		public IEnumerable<BinaryOperator> Operators => operators;

		public BinaryOperatorContainor() {
			operators = new HashSet<BinaryOperator>();

			AddBinaryOperator("+", BinaryOperatorType.Addition, NumericDefinitions);
			AddBinaryOperator("-", BinaryOperatorType.Subtraction, NumericDefinitions);
			AddBinaryOperator("*", BinaryOperatorType.Multiplication, NumericDefinitions);
			AddBinaryOperator("/", BinaryOperatorType.Division, NumericDefinitions);
			AddBinaryOperator("^", BinaryOperatorType.Power, NumericDefinitions);
			AddBinaryOperator("mod", BinaryOperatorType.Modulus, NumericDefinitions);

			AddBinaryOperator("==", BinaryOperatorType.Equality, EqualityDifinitions);

			AddBinaryOperator("&&", BinaryOperatorType.LogicalAnd, BooleanDefinitions);
			AddBinaryOperator("||", BinaryOperatorType.LogicalOr, BooleanDefinitions);
			AddBinaryOperator("^", BinaryOperatorType.LogicalXOr, BooleanDefinitions);
		}

		private void AddBinaryOperator(string token, BinaryOperatorType type, IEnumerable<Tuple<BoundType, BoundType, BoundType>> typeDefinitions) {
			foreach(var tuple in typeDefinitions) {
				operators.Add(new BinaryOperator(token, type, tuple.Item1, tuple.Item2, tuple.Item3));
			}
		}

		private void AddBinaryOperator(string token, BinaryOperatorType type, IEnumerable<Tuple<BoundType, BoundType>> typeDefinitions) {
			foreach (var tuple in typeDefinitions) {
				operators.Add(new BinaryOperator(token, type, tuple.Item1, tuple.Item2));
			}
		}

		private static Tuple<BoundType, BoundType, BoundType>[] NumericDefinitions = {
			Tuple.Create(BoundType.Integer, BoundType.Integer, BoundType.Integer),
			Tuple.Create(BoundType.Integer, BoundType.Double, BoundType.Double),
			Tuple.Create(BoundType.Integer, BoundType.Byte, BoundType.Integer),
			Tuple.Create(BoundType.Double, BoundType.Integer, BoundType.Double),
			Tuple.Create(BoundType.Double, BoundType.Double, BoundType.Double),
			Tuple.Create(BoundType.Double, BoundType.Byte, BoundType.Double),
			Tuple.Create(BoundType.Byte, BoundType.Integer, BoundType.Integer),
			Tuple.Create(BoundType.Byte, BoundType.Double, BoundType.Double),
			Tuple.Create(BoundType.Byte, BoundType.Byte, BoundType.Byte)
		};

		private static Tuple<BoundType, BoundType>[] EqualityDifinitions = {
			Tuple.Create(BoundType.Boolean, BoundType.Boolean),
			Tuple.Create(BoundType.Byte, BoundType.Boolean),
			Tuple.Create(BoundType.Double, BoundType.Boolean),
			Tuple.Create(BoundType.Integer, BoundType.Boolean),
			Tuple.Create(BoundType.String, BoundType.Boolean)
		};

		private static Tuple<BoundType, BoundType>[] BooleanDefinitions = {
			Tuple.Create(BoundType.Boolean, BoundType.Boolean)
		};
	}
}
