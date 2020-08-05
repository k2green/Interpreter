using InterpreterLib.Syntax.Tree.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Syntax.Tree.TypeDescriptions {
	internal sealed class ArrayTypeSyntax : TypeDescriptionSyntax {
		public override SyntaxType Type => SyntaxType.ArrayType;

		public override IEnumerable<SyntaxNode> Children {
			get {
				yield return ArrayToken;
				yield return LeftBracket;
				yield return ValueType;
				yield return RightBracket;
			}
		}

		public TokenSyntax ArrayToken { get; }
		public TokenSyntax LeftBracket { get; }
		public TypeDescriptionSyntax ValueType { get; }
		public TokenSyntax RightBracket { get; }

		public ArrayTypeSyntax(TokenSyntax arrayToken, TokenSyntax leftBracket, TypeDescriptionSyntax valueType, TokenSyntax rightBracket) {
			ArrayToken = arrayToken;
			LeftBracket = leftBracket;
			ValueType = valueType;
			RightBracket = rightBracket;
		}
	}
}
