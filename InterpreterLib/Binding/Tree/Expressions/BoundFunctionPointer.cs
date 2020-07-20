using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using InterpreterLib.Symbols.Binding;
using InterpreterLib.Symbols.Types;
using System.Collections.Immutable;

namespace InterpreterLib.Binding.Tree.Expressions {
	internal sealed class BoundFunctionPointer : BoundExpression {

		public override NodeType Type => NodeType.FunctionPointer;
		public override TypeSymbol ValueType { get; }

		public FunctionSymbol Function { get; }

		public BoundFunctionPointer(FunctionSymbol function) {
			Function = function;

			var paramTypes = function.Parameters.Select(param => param.ValueType).ToImmutableArray();
			ValueType = new FunctionTypeSymbol(paramTypes, function.ReturnType);
		}

		public override string ToString() => Function.Name;
	}
}
