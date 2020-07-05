using InterpreterLib.Binding.Tree.Expressions;
using InterpreterLib.Binding.Types;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace InterpreterLib.Binding.Tree.Expressions {
	internal sealed class BoundFunctionCall : BoundExpression {
		public override NodeType Type => NodeType.FunctionCall;

		public FunctionSymbol Function { get; }
		public IReadOnlyList<BoundExpression> Parameters { get; }

		public override TypeSymbol ValueType => Function.ReturnType;

		public BoundFunctionCall(FunctionSymbol function, IReadOnlyList<BoundExpression> parameters) {
			Function = function;
			Parameters = parameters;
		}
	}
}
