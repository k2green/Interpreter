using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace InterpreterLib.Types {
	public sealed class FunctionSymbol : Symbol {

		public override SymbolType Type => SymbolType.Function;
		public override string Name { get; }
		public ImmutableArray<ParameterSymbol> Parameters { get; }
		public TypeSymbol ReturnType { get; }
		public LabelSymbol EndLabel => new LabelSymbol("FunctionEnd");

		public FunctionSymbol(string name, ImmutableArray<ParameterSymbol> parameters, TypeSymbol returnType) {
			Name = name;
			Parameters = parameters;
			ReturnType = returnType;
		}

		public override bool Equals(object obj) {
			if (!(obj is FunctionSymbol symbol)) return false;

			if (!Name.Equals(symbol.Name) || ReturnType != symbol.ReturnType)
				return false;

			if (Parameters.Length != symbol.Parameters.Length)
				return false;

			for (int index = 0; index < Parameters.Length; index++) {
				if (!Parameters[index].Equals(symbol.Parameters[index]))
					return false;
			}

			return true;
		}

		public override int GetHashCode() {
			return HashCode.Combine(Name, ReturnType);
		}
	}
}
