using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Binding.Tree.Statements {
	internal sealed class BoundBreakStatement : BoundStatement {
		public override NodeType Type => NodeType.Break;
	}
}
