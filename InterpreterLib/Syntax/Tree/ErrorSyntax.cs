using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Syntax.Tree {
	internal sealed class ErrorSyntax : SyntaxNode {

		public override SyntaxType Type => SyntaxType.Error;
		public override IEnumerable<SyntaxNode> Children => throw new Exception("Errors have no children");

		public ErrorSyntax(Diagnostic diagnostic) {
			Diagnostic = diagnostic;
		}

		public string CausingText { get; }
		public Diagnostic Diagnostic { get; }
	}
}
