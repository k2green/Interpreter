using InterpreterLib.Syntax.Tree.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Syntax.Tree.Patterns {
	internal sealed class VariablePatternSyntax : PatternSyntax {
		public override SyntaxType Type => SyntaxType.VariablePattern;
		public override TextSpan Span => Identifier.Span;

		public override IEnumerable<SyntaxNode> Children {
			get {
				yield return Identifier;
			}
		}

		public TokenSyntax Identifier { get; }

		public VariablePatternSyntax(TokenSyntax identifier) {
			Identifier = identifier;
		}
	}
}
