using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
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

		public override TextLocation Location => Statements.First().Location;
		public override TextSpan Span => new TextSpan(Statements.First().Span.Start, Statements.Last().Span.End);

		public ImmutableArray<GlobalSyntax> Statements { get; }

		public CompilationUnitSyntax(ImmutableArray<GlobalSyntax> statements) {
			Statements = statements;
		}
	}
}
