using InterpreterLib.Syntax.Tree.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Syntax.Tree.TypeDescriptions {
	internal sealed class TupleTypeSyntax : TypeDescriptionSyntax {
		public override SyntaxType Type => SyntaxType.TupleType;

		public override IEnumerable<SyntaxNode> Children {
			get {
				yield return LeftParen;

				foreach (var item in Types)
					yield return item;

				yield return LeftParen;
			}
		}


		public TokenSyntax LeftParen { get; }
		public SeperatedSyntaxList<SyntaxNode> Types { get; }
		public TokenSyntax RightParen { get; }

		public TupleTypeSyntax(TokenSyntax leftParen, SeperatedSyntaxList<SyntaxNode> types, TokenSyntax rightParen) {
			LeftParen = leftParen;
			Types = types;
			RightParen = rightParen;
		}
	}
}
