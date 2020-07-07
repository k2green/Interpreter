using InterpreterLib.Syntax.Tree.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Syntax.Tree {
	internal sealed class ParameterDefinitionSyntax : SyntaxNode {
		public override SyntaxType Type => SyntaxType.ParameterDefinition;

		public override IEnumerable<SyntaxNode> Children {
			get {
				yield return LeftParen;

				foreach (var param in Parameters)
					yield return param;

				yield return RightParen;
			}
		}

		public override TextLocation Location => LeftParen.Location;

		public TokenSyntax LeftParen { get; }
		public SeperatedSyntaxList<TypedIdentifierSyntax> Parameters { get; }
		public TokenSyntax RightParen { get; }

		public ParameterDefinitionSyntax(TokenSyntax leftParen, SeperatedSyntaxList<TypedIdentifierSyntax> parameters, TokenSyntax rightParen) {
			LeftParen = leftParen;
			Parameters = parameters;
			RightParen = rightParen;
		}

		public override string ToString() {
			var builder = new StringBuilder();

			builder.Append(LeftParen.ToString());
			builder.Append(Parameters.ToString());
			builder.Append(RightParen.ToString());

			return builder.ToString();
		}
	}
}
