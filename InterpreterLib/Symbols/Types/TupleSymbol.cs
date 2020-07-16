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
		public override BoundScope Scope { get; }

		public TupleSymbol(ImmutableArray<TypeSymbol> types, BoundScope parentScope, bool isReadOnly = false) {
			Types = types;
			Scope = new BoundScope(parentScope);

			for (var index = 0; index < Types.Length; index++) {
				var variable = new VariableSymbol($"Item{index + 1}", isReadOnly, Types[index]);
				Scope.TryDefineVariable(variable);
			}
		}

		public override bool Equals(object obj) {
			if (!(obj is TupleSymbol symbol)) return false;

			return Types.Equals(symbol.Types);
		}

		public override int GetHashCode() {
			return Types.GetHashCode();
		}
	}
}
