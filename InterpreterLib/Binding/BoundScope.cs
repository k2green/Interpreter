using InterpreterLib.Binding.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Binding {
	internal sealed class BoundScope {

		private Dictionary<string, VariableSymbol> variables;
		public BoundScope Parent;

		internal BoundScope(BoundScope parent) {
			Parent = parent;
			variables = new Dictionary<string, VariableSymbol>();
		}

		internal BoundScope() : this(null) { }

		internal bool TryDefine(VariableSymbol variable) {
			if (variables.ContainsKey(variable.Name))
				return false;

			variables.Add(variable.Name, variable);
			return true;
		}

		internal bool TryLookup(string name, out VariableSymbol variable) {
			if (variables.TryGetValue(name, out variable))
				return true;

			if (Parent == null)
				return false;

			return Parent.TryLookup(name, out variable);
		}

		internal VariableSymbol this[string name] => variables[name];
		internal bool TryDirectLookup(string name, out VariableSymbol variable) => variables.TryGetValue(name, out variable);

		public IEnumerable<VariableSymbol> GetVariables() {
			return variables.Values;
		}
	}
}
