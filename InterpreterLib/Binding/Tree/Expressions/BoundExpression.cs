
using InterpreterLib.Binding.Tree.Statements;
using InterpreterLib.Symbols.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Binding.Tree.Expressions {
	internal abstract class BoundExpression : BoundNode {
		public abstract TypeSymbol ValueType { get; }

		private static BoundExpression Error(Diagnostic diagnostic) {
			return new BoundErrorExpression(diagnostic);
		}

		public static BoundExpression Error(Diagnostic diagnostic, ref DiagnosticContainer diagnostics) {
			diagnostics.AddDiagnostic(diagnostic);

			return Error(diagnostic);
		}

		public static BoundExpression ConvertError(BoundErrorStatement error) => Error(error.Diagnostic);
	}
}
