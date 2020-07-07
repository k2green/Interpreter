using System;
using System.Collections.Generic;
using System.Text;
using InterpreterLib.Syntax.Tree.Global;

namespace InterpreterLib.Syntax.Tree.Global {
	internal sealed class CompilationUnitSyntax : SyntaxNode {
		public override SyntaxType Type => SyntaxType.CompilationUnit;

		public override IEnumerable<SyntaxNode> Children {
			get {
				foreach (var syntax in Statements)
					yield return syntax;
			}
		}

		public override TextLocation Location => new TextLocation(0, 0);

		public IEnumerable<GlobalSyntax> Statements { get; }

		public CompilationUnitSyntax(IEnumerable<GlobalSyntax> statements) {
			Statements = statements;
		}
	}
}
