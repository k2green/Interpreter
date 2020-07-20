using InterpreterLib.Binding.Tree.Expressions;
using InterpreterLib.Symbols.Binding;
using InterpreterLib.Symbols.Types;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Text;
using System.Linq;

namespace InterpreterLib.Binding.Tree.Expressions {
	internal sealed class BoundFunctionCall : BoundExpression {
		public override NodeType Type => NodeType.FunctionCall;

		public FunctionSymbol Function { get; }
		public VariableSymbol<FunctionTypeSymbol> PointerSymbol { get; }
		public ImmutableArray<BoundExpression> Parameters { get; }

		public override TypeSymbol ValueType => Function != null ? Function.ReturnType : PointerSymbol.ValueType.ReturnType;

		public BoundFunctionCall(FunctionSymbol function, VariableSymbol<FunctionTypeSymbol> pointerSymbol, ImmutableArray<BoundExpression> parameters) {
			Function = function;
			PointerSymbol = pointerSymbol;
			Parameters = parameters;
		}

		public string Name => Function != null ? Function.Name : PointerSymbol.Name;
		public TypeSymbol ReturnType => Function != null ? Function.ReturnType : PointerSymbol.ValueType.ReturnType;
		public ImmutableArray<TypeSymbol> ParameterTypes =>
			PointerSymbol != null ? PointerSymbol.ValueType.ParamTypes : Function.Parameters.Select(param => param.ValueType).ToImmutableArray();


		public override string ToString() {
			var builder = new StringBuilder();
			builder.Append(Name).Append("(");

			var paramCount = ParameterTypes.Length;
			for (int index = 0; index < paramCount; index++) {
				builder.Append(ParameterTypes[index].ToString());

				if (index < paramCount - 1)
					builder.Append(", ");
			}

			return builder.Append(") => ").Append(ReturnType).ToString();
		}
	}
}
