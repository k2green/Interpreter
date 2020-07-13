
using InterpreterLib.Symbols.Types;
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
			AddBinaryOperator("!=", BinaryOperatorType.NegatveEquality, EqualityDifinitions);

			AddBinaryOperator(">=", BinaryOperatorType.GreaterThan, ComparisonDefinitions);
			AddBinaryOperator("<=", BinaryOperatorType.LesserThan, ComparisonDefinitions);
			AddBinaryOperator(">", BinaryOperatorType.StrictGreaterThan, ComparisonDefinitions);
			AddBinaryOperator("<", BinaryOperatorType.StrinLesserThan, ComparisonDefinitions);

			AddBinaryOperator("&&", BinaryOperatorType.LogicalAnd, BooleanDefinitions);
			AddBinaryOperator("||", BinaryOperatorType.LogicalOr, BooleanDefinitions);
			AddBinaryOperator("^", BinaryOperatorType.LogicalXOr, BooleanDefinitions);
		}

		private void AddBinaryOperator(string token, BinaryOperatorType type, IEnumerable<Tuple<ValueTypeSymbol, ValueTypeSymbol, ValueTypeSymbol>> typeDefinitions) {
			foreach (var tuple in typeDefinitions) {
				operators.Add(new BinaryOperator(token, type, tuple.Item1, tuple.Item2, tuple.Item3));
			}
		}

		private void AddBinaryOperator(string token, BinaryOperatorType type, IEnumerable<Tuple<ValueTypeSymbol, ValueTypeSymbol>> typeDefinitions) {
			foreach (var tuple in typeDefinitions) {
				operators.Add(new BinaryOperator(token, type, tuple.Item1, tuple.Item2));
			}
		}

		private static Tuple<ValueTypeSymbol, ValueTypeSymbol, ValueTypeSymbol>[] NumericDefinitions = {
			Tuple.Create(ValueTypeSymbol.Integer, ValueTypeSymbol.Integer, ValueTypeSymbol.Integer),
			Tuple.Create(ValueTypeSymbol.Integer, ValueTypeSymbol.Double, ValueTypeSymbol.Double),
			Tuple.Create(ValueTypeSymbol.Integer, ValueTypeSymbol.Byte, ValueTypeSymbol.Integer),
			Tuple.Create(ValueTypeSymbol.Double, ValueTypeSymbol.Integer, ValueTypeSymbol.Double),
			Tuple.Create(ValueTypeSymbol.Double, ValueTypeSymbol.Double, ValueTypeSymbol.Double),
			Tuple.Create(ValueTypeSymbol.Double, ValueTypeSymbol.Byte, ValueTypeSymbol.Double),
			Tuple.Create(ValueTypeSymbol.Byte, ValueTypeSymbol.Integer, ValueTypeSymbol.Integer),
			Tuple.Create(ValueTypeSymbol.Byte, ValueTypeSymbol.Double, ValueTypeSymbol.Double),
			Tuple.Create(ValueTypeSymbol.Byte, ValueTypeSymbol.Byte, ValueTypeSymbol.Byte)
		};

		private static Tuple<ValueTypeSymbol, ValueTypeSymbol, ValueTypeSymbol>[] ConcatonationDefinitions = {
			Tuple.Create(ValueTypeSymbol.String, ValueTypeSymbol.String, ValueTypeSymbol.String),
			Tuple.Create(ValueTypeSymbol.Integer, ValueTypeSymbol.String, ValueTypeSymbol.String),
			Tuple.Create(ValueTypeSymbol.Double, ValueTypeSymbol.String, ValueTypeSymbol.String),
			Tuple.Create(ValueTypeSymbol.Byte, ValueTypeSymbol.String, ValueTypeSymbol.String),
			Tuple.Create(ValueTypeSymbol.Boolean, ValueTypeSymbol.String, ValueTypeSymbol.String),
			Tuple.Create(ValueTypeSymbol.Character, ValueTypeSymbol.String, ValueTypeSymbol.String),
			Tuple.Create(ValueTypeSymbol.String, ValueTypeSymbol.Integer, ValueTypeSymbol.String),
			Tuple.Create(ValueTypeSymbol.String, ValueTypeSymbol.Double, ValueTypeSymbol.String),
			Tuple.Create(ValueTypeSymbol.String, ValueTypeSymbol.Byte, ValueTypeSymbol.String),
			Tuple.Create(ValueTypeSymbol.String, ValueTypeSymbol.Boolean, ValueTypeSymbol.String),
			Tuple.Create(ValueTypeSymbol.String, ValueTypeSymbol.Character, ValueTypeSymbol.String),
			Tuple.Create(ValueTypeSymbol.Character, ValueTypeSymbol.Character, ValueTypeSymbol.String)
		};

		private static Tuple<ValueTypeSymbol, ValueTypeSymbol, ValueTypeSymbol>[] ComparisonDefinitions = {
			Tuple.Create(ValueTypeSymbol.Integer, ValueTypeSymbol.Integer, ValueTypeSymbol.Boolean),
			Tuple.Create(ValueTypeSymbol.Integer, ValueTypeSymbol.Double, ValueTypeSymbol.Boolean),
			Tuple.Create(ValueTypeSymbol.Integer, ValueTypeSymbol.Byte, ValueTypeSymbol.Boolean),
			Tuple.Create(ValueTypeSymbol.Double, ValueTypeSymbol.Integer, ValueTypeSymbol.Boolean),
			Tuple.Create(ValueTypeSymbol.Double, ValueTypeSymbol.Double, ValueTypeSymbol.Boolean),
			Tuple.Create(ValueTypeSymbol.Double, ValueTypeSymbol.Byte, ValueTypeSymbol.Boolean),
			Tuple.Create(ValueTypeSymbol.Byte, ValueTypeSymbol.Integer, ValueTypeSymbol.Boolean),
			Tuple.Create(ValueTypeSymbol.Byte, ValueTypeSymbol.Double, ValueTypeSymbol.Boolean),
			Tuple.Create(ValueTypeSymbol.Byte, ValueTypeSymbol.Byte, ValueTypeSymbol.Boolean)
		};

		private static Tuple<ValueTypeSymbol, ValueTypeSymbol, ValueTypeSymbol>[] EqualityDifinitions = {
			Tuple.Create(ValueTypeSymbol.Boolean, ValueTypeSymbol.Boolean, ValueTypeSymbol.Boolean),
			Tuple.Create(ValueTypeSymbol.Byte, ValueTypeSymbol.Byte, ValueTypeSymbol.Boolean),
			Tuple.Create(ValueTypeSymbol.Double, ValueTypeSymbol.Double, ValueTypeSymbol.Boolean),
			Tuple.Create(ValueTypeSymbol.Integer, ValueTypeSymbol.Integer, ValueTypeSymbol.Boolean),
			Tuple.Create(ValueTypeSymbol.String, ValueTypeSymbol.String, ValueTypeSymbol.Boolean)
		};

		private static Tuple<ValueTypeSymbol, ValueTypeSymbol, ValueTypeSymbol>[] BooleanDefinitions = {
			Tuple.Create(ValueTypeSymbol.Boolean, ValueTypeSymbol.Boolean, ValueTypeSymbol.Boolean)
		};
	}
}
