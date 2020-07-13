using InterpreterLib.Binding.Tree;
using InterpreterLib.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Diagnostics {
	internal static class OperatorEvaluator {

		public static object EvaluateAddition(object left, object right, BinaryOperator op) {
			if (op.OutputType == TypeSymbol.Double)
				return (double)left + (double)right;

			if (op.OutputType == TypeSymbol.Integer)
				return (int)left + (int)right;

			if (op.OutputType == TypeSymbol.Byte)
				return (byte)left + (byte)right;

			throw new Exception("Unhandled bonary operation case");
		}

		public static object EvaluateSubtraction(object left, object right, BinaryOperator op) {
			if (op.OutputType == TypeSymbol.Double)
				return (double)left - (double)right;

			if (op.OutputType == TypeSymbol.Integer)
				return (int)left - (int)right;

			if (op.OutputType == TypeSymbol.Byte)
				return (byte)left - (byte)right;

			throw new Exception("Unhandled bonary operation case");
		}

		public static object EvaluateMultiplication(object left, object right, BinaryOperator op) {
			if (op.OutputType == TypeSymbol.Double)
				return (double)left * (double)right;

			if (op.OutputType == TypeSymbol.Integer)
				return (int)left * (int)right;

			if (op.OutputType == TypeSymbol.Byte)
				return (byte)left * (byte)right;

			throw new Exception("Unhandled bonary operation case");
		}

		public static object EvaluateDivision(object left, object right, BinaryOperator op) {
			if (op.OutputType == TypeSymbol.Double)
				return (double)left / (double)right;

			if (op.OutputType == TypeSymbol.Integer)
				return (int)left / (int)right;

			if (op.OutputType == TypeSymbol.Byte)
				return (byte)left / (byte)right;

			throw new Exception("Unhandled bonary operation case");
		}

		public static object EvaluatePower(object left, object right, BinaryOperator op) {
			if (op.OutputType == TypeSymbol.Double)
				return Math.Pow((double)left, (double)right);

			if (op.OutputType == TypeSymbol.Integer)
				return (int)Math.Pow((int)left, (int)right);

			if (op.OutputType == TypeSymbol.Byte)
				return (byte)Math.Pow((byte)left, (byte)right);

			throw new Exception("Unhandled bonary operation case");
		}

		internal static object EvaluateGreaterThan(object left, object right, BinaryOperator op) {
			if (op.LeftType == TypeSymbol.Double || op.RightType == TypeSymbol.Double)
				return (double)left >= (double)right;

			if (op.LeftType == TypeSymbol.Integer || op.RightType == TypeSymbol.Integer)
				return (int)left >= (int)right;

			if (op.LeftType == TypeSymbol.Byte || op.RightType == TypeSymbol.Byte)
				return (byte)left >= (byte)right;

			throw new Exception("Unhandled bonary operation case");
		}

		internal static object EvaluateLesserThan(object left, object right, BinaryOperator op) {
			if (op.LeftType == TypeSymbol.Double || op.RightType == TypeSymbol.Double)
				return (double)left <= (double)right;

			if (op.LeftType == TypeSymbol.Integer || op.RightType == TypeSymbol.Integer)
				return (int)left <= (int)right;

			if (op.LeftType == TypeSymbol.Byte || op.RightType == TypeSymbol.Byte)
				return (byte)left <= (byte)right;

			throw new Exception("Unhandled bonary operation case");
		}

		internal static object EvaluateStrictGreaterThan(object left, object right, BinaryOperator op) {
			if (op.LeftType == TypeSymbol.Double || op.RightType == TypeSymbol.Double)
				return (double)left > (double)right;

			if (op.LeftType == TypeSymbol.Integer || op.RightType == TypeSymbol.Integer)
				return (int)left > (int)right;

			if (op.LeftType == TypeSymbol.Byte || op.RightType == TypeSymbol.Byte)
				return (byte)left > (byte)right;

			throw new Exception("Unhandled bonary operation case");
		}

		internal static object EvaluateStrictLesserThan(object left, object right, BinaryOperator op) {
			if (op.LeftType == TypeSymbol.Double || op.RightType == TypeSymbol.Double)
				return (double)left < (double)right;

			if (op.LeftType == TypeSymbol.Integer || op.RightType == TypeSymbol.Integer)
				return (int)left < (int)right;

			if (op.LeftType == TypeSymbol.Byte || op.RightType == TypeSymbol.Byte)
				return (byte)left < (byte)right;

			throw new Exception("Unhandled bonary operation case");
		}

		internal static bool EvaluateEquality(object left, object right, BinaryOperator op) {
			return left.Equals(right);
		}

		internal static object EvaluateConcatonation(object left, object right) {
			return new StringBuilder().Append(left).Append(right).ToString();
		}
	}
}
