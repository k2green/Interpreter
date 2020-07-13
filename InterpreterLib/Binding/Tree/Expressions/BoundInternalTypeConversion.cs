using InterpreterLib.Symbols.Binding;
using InterpreterLib.Symbols.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Binding.Tree.Expressions {
	internal sealed class BoundInternalTypeConversion : BoundExpression {
		public override ValueTypeSymbol ValueType => ConversionSymbol.ToType;

		public override NodeType Type => NodeType.InternalTypeConversion;

		public TypeConversionSymbol ConversionSymbol { get; }
		public BoundExpression Expression { get; }

		public BoundInternalTypeConversion(TypeConversionSymbol conversionSymbol, BoundExpression expression) {
			ConversionSymbol = conversionSymbol;
			Expression = expression;
		}

		public override string ToString() => ConversionSymbol.ToType.ToString() + "(" + Expression.ToString() + ")";
	}
}
