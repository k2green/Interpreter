using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Binding.Tree.Statements {
	internal sealed class BoundLabel : BoundStatement {
		public override NodeType Type => NodeType.Label;

		public string Name { get; }

		public BoundLabel(string name) {
			Name = name;
		}
	}
}
