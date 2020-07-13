using InterpreterLib.Symbols;
using InterpreterLib.Symbols.Binding;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InterpreterLib.Binding {
	internal sealed class BoundScope {

		private Dictionary<string, Symbol> symbols;
		public BoundScope Parent;

		internal BoundScope(BoundScope parent) {
			Parent = parent;
			symbols = new Dictionary<string, Symbol>();
		}

		internal BoundScope() : this(null) { }

		internal bool TryDefineVariable(VariableSymbol variable) => TryDefineSymbol(variable);

		internal bool TryLookupVariable(string name, out VariableSymbol variable) => TryLookupSymbol(name, out variable);

		internal bool TryDefineFunction(FunctionSymbol function) => TryDefineSymbol(function);

		internal bool TryLookupFunction(string name, out FunctionSymbol function) => TryLookupSymbol(name, out function);

		public bool TryDefineSymbol<T>(T symbol) where T : Symbol {
			if (symbols.ContainsKey(symbol.Name))
				return false;

			symbols.Add(symbol.Name, symbol);
			return true;
		}

		public bool TryLookupSymbol<T>(string name, out T symbol) where T : Symbol {
			symbol = null;

			if (symbols.TryGetValue(name, out var lookup)) {
				if (!(lookup is T castedLookup))
					return false;

				symbol = castedLookup;
				return true;
			}

			if (Parent == null)
				return false;

			return Parent.TryLookupSymbol(name, out symbol);
		}

		public IEnumerable<VariableSymbol> GetVariables() {
			return symbols.Values.OfType<VariableSymbol>();
		}

		public IEnumerable<FunctionSymbol> GetFunctions() {
			return symbols.Values.OfType<FunctionSymbol>();
		}
	}
}
