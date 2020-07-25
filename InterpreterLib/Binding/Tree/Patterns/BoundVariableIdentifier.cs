using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Binding.Tree.Patterns {
	internal sealed class BoundVariableIdentifier : BoundIdentifierPattern {
		public override NodeType Type => NodeType.VariableIdentifier;

		public string VarName { get; }

		public BoundVariableIdentifier(string varName) {
			VarName = varName;
		}

		public override string ToString() => VarName;
	}
}
