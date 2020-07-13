using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Binding.Tree {
	internal abstract class BoundNode {
		public abstract NodeType Type { get; }

		public abstract override string ToString();

		public string StringFormat(string input) => $"\"{input}\"";
	}
}
