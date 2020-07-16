using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Syntax.Tree.Expressions {
	internal sealed class AccessorSyntax : ExpressionSyntax {
		public override SyntaxType Type => SyntaxType.Accessor;
		public override TextLocation Location => Item.Location;
		public override TextSpan Span => IsLast ? Item.Span : new TextSpan(Item.Span.Start, Rest.Span.End);

		public override IEnumerable<SyntaxNode> Children {
			get {
				yield return Item;

				if (!IsLast) {
					yield return Comma;
					yield return Rest;
				}
			}
		}

		public AccessorExpressionSyntax Item { get; }
		public TokenSyntax Comma { get; }
		public AccessorSyntax Rest { get; }
		public bool IsLast => Rest == null;

		public AccessorSyntax(AccessorExpressionSyntax item, TokenSyntax comma, AccessorSyntax rest) {
			Item = item;
			Comma = comma;
			Rest = rest;
		}
	}
}
