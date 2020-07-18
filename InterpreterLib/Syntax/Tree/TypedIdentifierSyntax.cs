using InterpreterLib.Syntax.Tree.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Syntax.Tree {
	internal sealed class TypedIdentifierSyntax : SyntaxNode {
		public override SyntaxType Type => SyntaxType.TypedIdentifier;

		public override IEnumerable<SyntaxNode> Children {
			get {
				yield return IdentifierName;
				yield return Definition;
			}
		}

		public override TextLocation Location => IdentifierName.Location;

		public TokenSyntax IdentifierName { get; }
		public TypeDefinitionSyntax Definition { get; }

		public override TextSpan Span => new TextSpan(IdentifierName.Span.Start, IdentifierName.Span.End);

		public TypedIdentifierSyntax(TokenSyntax identifier, TypeDefinitionSyntax definition) {
			IdentifierName = identifier;
			Definition = definition;
		}
	}
}
