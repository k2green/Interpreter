using InterpreterLib.Binding.Tree.Expressions;
using InterpreterLib.Types;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Text;

namespace InterpreterLib.Binding.Tree.Expressions {
	internal sealed class BoundFunctionCall : BoundExpression {
		public override NodeType Type => NodeType.FunctionCall;

		public FunctionSymbol Function { get; }
		public ImmutableArray<BoundExpression> Parameters { get; }

		public override TypeSymbol ValueType => Function.ReturnType;

		public BoundFunctionCall(FunctionSymbol function, ImmutableArray<BoundExpression> parameters) {
			Function = function;
			Parameters = parameters;
		}
	}
}
