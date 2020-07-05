using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Binding.Types {
	public sealed class FunctionSymbol : Symbol {

		public override SymbolType Type => SymbolType.Function;
		public override string Name { get; }
		public IReadOnlyList<ParameterSymbol> Parameters { get; }
		public TypeSymbol ReturnType { get; }

		public FunctionSymbol(string name, IReadOnlyList<ParameterSymbol> parameters, TypeSymbol returnType) {
			Name = name;
			Parameters = parameters;
			ReturnType = returnType;
		}

		public override bool Equals(object obj) {
			if (!(obj is FunctionSymbol symbol)) return false;

			if (!Name.Equals(symbol.Name) || ReturnType != symbol.ReturnType)
				return false;

			if (Parameters.Count != symbol.Parameters.Count)
				return false;

			for (int index = 0; index < Parameters.Count; index++) {
				if (!Parameters[index].Equals(symbol.Parameters[index]))
					return false;
			}

			return true;
		}

		public override int GetHashCode() {
			throw new NotImplementedException();
		}
	}
}
