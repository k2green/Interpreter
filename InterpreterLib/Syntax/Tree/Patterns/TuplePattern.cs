using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace InterpreterLib.Syntax.Tree.Patterns {
	internal sealed class TuplePatternSyntax : PatternSyntax {
		public override SyntaxType Type => SyntaxType.TuplePattern;

		public override IEnumerable<SyntaxNode> Children {
			get {
				foreach (var pattern in Patterns)
					yield return pattern;
			}
		}

		public SeperatedSyntaxList<PatternSyntax> Patterns { get; }

		public TuplePatternSyntax(SeperatedSyntaxList<PatternSyntax> patterns) {
			Patterns = patterns;
		}
	}
}
