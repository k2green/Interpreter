using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Binding {
	internal sealed class BoundScope {

		private Dictionary<string, BoundVariable> variables;
		private BoundScope parent;

		public BoundScope(BoundScope parent) {
			this.parent = parent;
			variables = new Dictionary<string, BoundVariable>();
		}

		public BoundScope() : this(null) { }

		public bool TryDefine(BoundVariable variable) {
			if (variables.ContainsKey(variable.Name))
				return false;

			variables.Add(variable.Name, variable);
			return true;
		}

		public bool TryLookup(string name, out BoundVariable variable) {
			if (variables.TryGetValue(name, out variable))
				return true;

			if (parent == null)
				return false;

			return parent.TryLookup(name, out variable);
		}

		public IEnumerable<BoundVariable> GetVariables() {
			return variables.Values;
		}
	}
}
