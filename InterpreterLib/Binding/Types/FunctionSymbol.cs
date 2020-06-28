using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Binding.Types {
	public sealed class FunctionSymbol : Symbol {

		public override SymbolType Type => SymbolType.Function;
		public override string Name { get; }

		public FunctionSymbol(string name, IReadOnlyList<ParameterSymbol> parameters, TypeSymbol returnType) {
			Name = name;
		}

		public override bool Equals(object obj) {
			throw new NotImplementedException();
		}

		public override int GetHashCode() {
			throw new NotImplementedException();
		}
	}
}
