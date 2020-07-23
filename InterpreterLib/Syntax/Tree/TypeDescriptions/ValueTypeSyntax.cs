using InterpreterLib.Syntax.Tree.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Syntax.Tree.TypeDescriptions {
	internal sealed class ValueTypeSyntax : TypeDescriptionSyntax {
		public override SyntaxType Type => SyntaxType.ValueType;

		public override IEnumerable<SyntaxNode> Children {
			get {
				yield return TypeName;
			}
		}

		public TokenSyntax TypeName { get; }

		public ValueTypeSyntax(TokenSyntax typeName) {
			TypeName = typeName;
		}
	}
}
