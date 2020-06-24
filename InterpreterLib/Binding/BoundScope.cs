using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Binding {
	internal sealed class BoundScope {

		private Dictionary<string, BoundVariable> variables;
		private BoundScope parent;

		internal BoundScope(BoundScope parent) {
			this.parent = parent;
			variables = new Dictionary<string, BoundVariable>();
		}

		internal BoundScope() : this(null) { }

		internal bool TryDefine(BoundVariable variable) {
			if (variables.ContainsKey(variable.Name))
				return false;

			variables.Add(variable.Name, variable);
			return true;
		}

		internal bool TryLookup(string name, out BoundVariable variable) {
			if (variables.TryGetValue(name, out variable))
				return true;

			if (parent == null)
				return false;

			return parent.TryLookup(name, out variable);
		}

		internal BoundVariable this[string name] => variables[name];
		internal bool TryDirectLookup(string name, out BoundVariable variable) => variables.TryGetValue(name, out variable);

		public IEnumerable<BoundVariable> GetVariables() {
			return variables.Values;
		}
	}
}
