using InterpreterLib.Binding;
using InterpreterLib.Symbols.Binding;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace InterpreterLib.Symbols.Types {
	internal sealed class TupleSymbol : AccessibleSymbol {
		public override SymbolType Type => SymbolType.Tuple;
		public override string Name => "Tuple";
		public ImmutableArray<TypeSymbol> Types { get; }
		public override IReadOnlyList<VariableSymbol> Variables => variables;

		private List<VariableSymbol> variables;

		public TupleSymbol(ImmutableArray<TypeSymbol> types, bool isReadOnly = false) {
			Types = types;
			variables = new List<VariableSymbol>();

			for (var index = 0; index < Types.Length; index++) {
				var variable = new VariableSymbol($"Item{index + 1}", isReadOnly, Types[index]);
				variables.Add(variable);
			}
		}

		public override bool Equals(object obj) {
			if (!(obj is TupleSymbol symbol)) return false;

			return Types.Equals(symbol.Types);
		}

		public override int GetHashCode() {
			return Types.GetHashCode();
		}

		public override string ToString() {
			var builder = new StringBuilder();

			builder.Append("(");

			for(int index = 0; index < Types.Length; index++) {
				builder.Append(Types[index]);

				if (index < Types.Length - 1)
					builder.Append(", ");
			}

			return builder.Append(")").ToString();
		}
	}
}
