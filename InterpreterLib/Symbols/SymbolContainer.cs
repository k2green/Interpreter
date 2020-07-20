using InterpreterLib.Symbols.Binding;
using InterpreterLib.Symbols.Types;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InterpreterLib.Symbols {
	internal class SymbolContainer {
		protected Dictionary<string, Symbol> symbols;

		public SymbolContainer() {
			symbols = new Dictionary<string, Symbol>();
		}

		internal virtual bool TryDefineVariable(VariableSymbol variable) => TryDefineSymbol(variable);

		internal virtual bool TryLookupVariable(string name, out VariableSymbol variable) => TryLookupSymbol(name, out variable);

		internal virtual bool TryLookupVariable<T>(string name, out VariableSymbol<T> variable) where T : TypeSymbol {
			if (TryLookupVariable(name, out var lookup) && lookup.ValueType is T castedType) {
				variable = new VariableSymbol<T>(name, lookup.IsReadOnly, castedType);
				return true;
			}

			variable = null;
			return false;
		}

		internal virtual bool TryDefineFunction(FunctionSymbol function) => TryDefineSymbol(function);

		internal virtual bool TryLookupFunction(string name, out FunctionSymbol function) => TryLookupSymbol(name, out function);

		internal virtual bool TryDefineSymbol<T>(T symbol) where T : Symbol {
			if (symbols.ContainsKey(symbol.Name))
				return false;

			symbols.Add(symbol.Name, symbol);
			return true;
		}

		internal virtual bool TryLookupSymbol<T>(string name, out T symbol) where T : Symbol {
			symbol = null;

			if (symbols.TryGetValue(name, out var lookup)) {
				if (!(lookup is T castedLookup))
					return false;

				symbol = castedLookup;
				return true;
			}

			return false;
		}

		public IEnumerable<Symbol> GetSymbols() => symbols.Values;
	}
}
