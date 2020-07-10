using InterpreterLib.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Binding.Tree.Statements {
	internal abstract class BoundLoopStatement : BoundStatement {
		public abstract LabelSymbol BreakLabel { get; }
		public abstract LabelSymbol ContinueLabel { get; }
	}
}
