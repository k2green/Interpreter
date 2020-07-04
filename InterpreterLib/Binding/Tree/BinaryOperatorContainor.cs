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

			AddBinaryOperator("+", BinaryOperatorType.Concatonate, ConcatonationDefinitions);

			AddBinaryOperator("+", BinaryOperatorType.Addition, NumericDefinitions);
			AddBinaryOperator("-", BinaryOperatorType.Subtraction, NumericDefinitions);
			AddBinaryOperator("*", BinaryOperatorType.Multiplication, NumericDefinitions);
			AddBinaryOperator("/", BinaryOperatorType.Division, NumericDefinitions);
			AddBinaryOperator("^", BinaryOperatorType.Power, NumericDefinitions);
			AddBinaryOperator("mod", BinaryOperatorType.Modulus, NumericDefinitions);

			AddBinaryOperator("==", BinaryOperatorType.Equality, EqualityDifinitions);

			AddBinaryOperator(">=", BinaryOperatorType.GreaterThan, ComparisonDefinitions);
			AddBinaryOperator("<=", BinaryOperatorType.LesserThan, ComparisonDefinitions);
			AddBinaryOperator(">", BinaryOperatorType.StrictGreaterThan, ComparisonDefinitions);
			AddBinaryOperator("<", BinaryOperatorType.StrinLesserThan, ComparisonDefinitions);

			AddBinaryOperator("&&", BinaryOperatorType.LogicalAnd, BooleanDefinitions);
			AddBinaryOperator("||", BinaryOperatorType.LogicalOr, BooleanDefinitions);
			AddBinaryOperator("^", BinaryOperatorType.LogicalXOr, BooleanDefinitions);
		}

		private void AddBinaryOperator(string token, BinaryOperatorType type, IEnumerable<Tuple<TypeSymbol, TypeSymbol, TypeSymbol>> typeDefinitions) {
			foreach (var tuple in typeDefinitions) {
				operators.Add(new BinaryOperator(token, type, tuple.Item1, tuple.Item2, tuple.Item3));
			}
		}

		private void AddBinaryOperator(string token, BinaryOperatorType type, IEnumerable<Tuple<TypeSymbol, TypeSymbol>> typeDefinitions) {
			foreach (var tuple in typeDefinitions) {
				operators.Add(new BinaryOperator(token, type, tuple.Item1, tuple.Item2));
			}
		}

		private static Tuple<TypeSymbol, TypeSymbol, TypeSymbol>[] NumericDefinitions = {
			Tuple.Create(TypeSymbol.Integer, TypeSymbol.Integer, TypeSymbol.Integer),
			Tuple.Create(TypeSymbol.Integer, TypeSymbol.Double, TypeSymbol.Double),
			Tuple.Create(TypeSymbol.Integer, TypeSymbol.Byte, TypeSymbol.Integer),
			Tuple.Create(TypeSymbol.Double, TypeSymbol.Integer, TypeSymbol.Double),
			Tuple.Create(TypeSymbol.Double, TypeSymbol.Double, TypeSymbol.Double),
			Tuple.Create(TypeSymbol.Double, TypeSymbol.Byte, TypeSymbol.Double),
			Tuple.Create(TypeSymbol.Byte, TypeSymbol.Integer, TypeSymbol.Integer),
			Tuple.Create(TypeSymbol.Byte, TypeSymbol.Double, TypeSymbol.Double),
			Tuple.Create(TypeSymbol.Byte, TypeSymbol.Byte, TypeSymbol.Byte)
		};

		private static Tuple<TypeSymbol, TypeSymbol, TypeSymbol>[] ConcatonationDefinitions = {
			Tuple.Create(TypeSymbol.String, TypeSymbol.String, TypeSymbol.String),
			Tuple.Create(TypeSymbol.Integer, TypeSymbol.String, TypeSymbol.String),
			Tuple.Create(TypeSymbol.Double, TypeSymbol.String, TypeSymbol.String),
			Tuple.Create(TypeSymbol.Byte, TypeSymbol.String, TypeSymbol.String),
			Tuple.Create(TypeSymbol.Boolean, TypeSymbol.String, TypeSymbol.String),
			Tuple.Create(TypeSymbol.String, TypeSymbol.Integer, TypeSymbol.String),
			Tuple.Create(TypeSymbol.String, TypeSymbol.Double, TypeSymbol.String),
			Tuple.Create(TypeSymbol.String, TypeSymbol.Byte, TypeSymbol.String),
			Tuple.Create(TypeSymbol.String, TypeSymbol.Boolean, TypeSymbol.String)
		};

		private static Tuple<TypeSymbol, TypeSymbol, TypeSymbol>[] ComparisonDefinitions = {
			Tuple.Create(TypeSymbol.Integer, TypeSymbol.Integer, TypeSymbol.Boolean)
		};

		private static Tuple<TypeSymbol, TypeSymbol, TypeSymbol>[] EqualityDifinitions = {
			Tuple.Create(TypeSymbol.Boolean, TypeSymbol.Boolean, TypeSymbol.Boolean),
			Tuple.Create(TypeSymbol.Byte, TypeSymbol.Byte, TypeSymbol.Boolean),
			Tuple.Create(TypeSymbol.Double, TypeSymbol.Double, TypeSymbol.Boolean),
			Tuple.Create(TypeSymbol.Integer, TypeSymbol.Integer, TypeSymbol.Boolean),
			Tuple.Create(TypeSymbol.String, TypeSymbol.String, TypeSymbol.Boolean)
		};

		private static Tuple<TypeSymbol, TypeSymbol, TypeSymbol>[] BooleanDefinitions = {
			Tuple.Create(TypeSymbol.Boolean, TypeSymbol.Boolean, TypeSymbol.Boolean)
		};
	}
}
