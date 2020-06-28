using InterpreterLib.Binding.Tree;
using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Binding.Lowering {
	internal class Lowerer : BoundTreeRewriter {

		protected override BoundStatement RewriteIfStatement(BoundIfStatement statement) {
			return base.RewriteIfStatement(statement);
		}
	}
}
