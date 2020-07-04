using System;
using System.Collections.Generic;
using System.Text;
using InterpreterLib.Binding.Types;

namespace InterpreterLib.Binding.Tree.Expressions {
	internal sealed class BoundInternalTypeConversion : BoundExpression {
		public override TypeSymbol ValueType => ConversionSymbol.ToType;

		public override NodeType Type => NodeType.InternalTypeConversion;

		public TypeConversionSymbol ConversionSymbol { get; }
		public BoundExpression Expression { get; }

		public BoundInternalTypeConversion(TypeConversionSymbol conversionSymbol, BoundExpression expression) {
			ConversionSymbol = conversionSymbol;
			Expression = expression;
		}
	}
}
