using InterpreterLib.Symbols;
using InterpreterLib.Symbols.Binding;
using InterpreterLib.Symbols.Types;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InterpreterLib.Binding {
	internal sealed class BoundScope : SymbolContainer {

		public BoundScope Parent;

		internal BoundScope(BoundScope parent) {
			Parent = parent;
			symbols = new Dictionary<string, Symbol>();
		}

		internal BoundScope() : this(null) { }

		internal override bool TryLookupSymbol<T>(string name, out T symbol) {
			if (base.TryLookupSymbol(name, out symbol))
				return true;

			if (Parent == null) {
				symbol = null;
				return false;
			}

			return Parent.TryLookupSymbol(name, out symbol);
		}

		public IEnumerable<VariableSymbol> GetVariables() {
			return GetSymbols().OfType<VariableSymbol>();
		}

		public IEnumerable<FunctionSymbol> GetFunctions() {
			return GetSymbols().OfType<FunctionSymbol>();
		}
	}
}
