using InterpreterLib.Syntax.Tree.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Syntax.Tree {
	internal class TypeDefinitionSyntax : SyntaxNode {
		public override SyntaxType Type => SyntaxType.TypeDefinition;

		public override IEnumerable<SyntaxNode> Children {
			get {
				yield return DelimeterToken;
				yield return NameToken;
			}
		}

		public TokenSyntax DelimeterToken { get; }
		public SyntaxNode NameToken { get; }
		public override TextLocation Location => DelimeterToken.Location;
		public override TextSpan Span => new TextSpan(DelimeterToken.Span.Start, NameToken.Span.End);

		public TypeDefinitionSyntax(TokenSyntax delimeterToken, SyntaxNode nameToken) {
			DelimeterToken = delimeterToken;
			NameToken = nameToken;
		}
	}
}
