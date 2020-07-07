using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Syntax.Tree {
	internal sealed class ErrorSyntax : SyntaxNode {

		public override SyntaxType Type => SyntaxType.Error;
		public override IEnumerable<SyntaxNode> Children => throw new Exception("Errors have no children");

		public Diagnostic Diagnostic { get; }
		public override TextLocation Span { get; }

		public ErrorSyntax(Diagnostic diagnostic) {
			Diagnostic = diagnostic;
			Span = new TextLocation(0, 0, diagnostic.Line, diagnostic.Column);
		}

		public override string ToString() {
			return Diagnostic.ToString();
		}
	}
}
