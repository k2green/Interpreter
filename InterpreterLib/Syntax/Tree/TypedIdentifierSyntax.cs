using InterpreterLib.Syntax.Tree.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Syntax.Tree {
	internal sealed class TypedIdentifierSyntax : SyntaxNode {
		public override SyntaxType Type => SyntaxType.TypedIdentifier;

		public override IEnumerable<SyntaxNode> Children {
			get {
				yield return Identifier;
				yield return Definition;
			}
		}

		public override TextSpan Span { get; }

		public TokenSyntax Identifier { get; }
		public TypeDefinitionSyntax Definition { get; }

		public TypedIdentifierSyntax(TokenSyntax identifier, TypeDefinitionSyntax definition) {
			Identifier = identifier;
			Definition = definition;

			Span = CreateNewSpan(identifier.Span, definition.Span);
		}

		public override string ToString() {
			throw new NotImplementedException();
		}
	}
}
