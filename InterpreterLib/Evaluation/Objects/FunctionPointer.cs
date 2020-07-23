using InterpreterLib.Symbols.Binding;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Collections.Immutable;

namespace InterpreterLib.Evaluation.Objects {
	internal class FunctionPointerObject : RuntimeObject {

		public FunctionSymbol Function { get; }
		public string Name { get; }

		public FunctionPointerObject(FunctionSymbol function, string name) {
			Function = function;
			Name = name;
		}

		public override string ToString() {
			var builder = new StringBuilder();
			var paramTypes = Function.Parameters.Select(param => param.ValueType).ToImmutableArray();

			builder.Append(Name).Append(" (");

			for (int index = 0; index < paramTypes.Length; index++) {
				builder.Append(paramTypes[index]);

				if (index < paramTypes.Length - 1)
					builder.Append(", ");
			}

			builder.Append(") => ").Append(Function.ReturnType);

			return builder.ToString();
		}
	}
}
