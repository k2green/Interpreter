using InterpreterLib.Binding.Tree.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Binding.Tree.Statements {
	internal abstract class BoundStatement : BoundNode {

		private static BoundStatement Error(Diagnostic diagnostic) {
			return new BoundErrorStatement(diagnostic);
		}

		public static BoundStatement Error(Diagnostic diagnostic, ref DiagnosticContainer diagnostics) {
			diagnostics.AddDiagnostic(diagnostic);

			return Error(diagnostic);
		}

		public static BoundStatement ConvertError(BoundErrorExpression error) => Error(error.Diagnostic);
	}
}
