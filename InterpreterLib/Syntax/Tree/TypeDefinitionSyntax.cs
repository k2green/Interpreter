using InterpreterLib.Syntax.Tree.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Syntax.Tree {
	internal sealed class TypeDefinitionSyntax : SyntaxNode {
		public override SyntaxType Type => SyntaxType.TypeDefinition;

		public override IEnumerable<SyntaxNode> Children {
			get {
				yield return DelimeterToken;
				yield return NameToken;
			}
		}

		public TokenSyntax DelimeterToken { get; }
		public TokenSyntax NameToken { get; }
		public override TextLocation Location => DelimeterToken.Location;

		public TypeDefinitionSyntax(TokenSyntax delimeterToken, TokenSyntax nameToken) {
			DelimeterToken = delimeterToken;
			NameToken = nameToken;
		}
	}
}
