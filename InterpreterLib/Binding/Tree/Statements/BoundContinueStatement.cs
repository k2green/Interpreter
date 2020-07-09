using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Binding.Tree.Statements {
	internal sealed class BoundContinueStatement : BoundStatement {
		public override NodeType Type => NodeType.Continue;
	}
}
