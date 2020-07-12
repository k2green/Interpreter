using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Binding.Tree.Statements {
	internal sealed class BoundErrorStatement : BoundStatement {
		public override NodeType Type => NodeType.Error;

		public Diagnostic Diagnostic { get; }

		public BoundErrorStatement(Diagnostic diagnostic) {
			Diagnostic = diagnostic;
		}
	}
}
