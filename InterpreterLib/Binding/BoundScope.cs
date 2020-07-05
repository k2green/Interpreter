using InterpreterLib.Binding.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Binding {
	internal sealed class BoundScope {

		private Dictionary<string, VariableSymbol> variables;
		private Dictionary<string, FunctionSymbol> functions;
		public BoundScope Parent;

		internal BoundScope(BoundScope parent) {
			Parent = parent;
			variables = new Dictionary<string, VariableSymbol>();
			functions = new Dictionary<string, FunctionSymbol>();
		}

		internal BoundScope() : this(null) { }

		internal bool TryDefineVariable(VariableSymbol variable) {
			if (variables.ContainsKey(variable.Name))
				return false;

			variables.Add(variable.Name, variable);
			return true;
		}

		internal bool TryLookupVariable(string name, out VariableSymbol variable) {
			if (variables.TryGetValue(name, out variable))
				return true;

			if (Parent == null)
				return false;

			return Parent.TryLookupVariable(name, out variable);
		}

		internal bool TryDefineFunction(FunctionSymbol function) {
			if (functions.ContainsKey(function.Name))
				return false;

			functions.Add(function.Name, function);
			return true;
		}

		internal bool TryLookupFunction(string name, out FunctionSymbol function) {
			if (functions.TryGetValue(name, out function))
				return true;

			if (Parent == null)
				return false;

			return Parent.TryLookupFunction(name, out function);
		}


		internal VariableSymbol this[string name] => variables[name];
		internal bool TryDirectLookup(string name, out VariableSymbol variable) => variables.TryGetValue(name, out variable);

		public IEnumerable<VariableSymbol> GetVariables() {
			return variables.Values;
		}
	}
}
