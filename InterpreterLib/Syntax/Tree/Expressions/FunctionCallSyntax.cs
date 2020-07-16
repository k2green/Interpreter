using InterpreterLib.Syntax.Tree.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Syntax.Tree.Expressions {
	internal sealed class FunctionCallSyntax : AccessorExpressionSyntax {
		public override SyntaxType Type => SyntaxType.FunctionCall;

		public override IEnumerable<SyntaxNode> Children {
			get {
				yield return Identifier;
				yield return LeftParen;

				foreach (var syntax in Parameters)
					yield return syntax;

				yield return RightParen;
			}
		}

		public override TextLocation Location => Identifier.Location;
		public override TextSpan Span => new TextSpan(Identifier.Span.Start, RightParen.Span.End);

		public TokenSyntax Identifier { get; }
		public TokenSyntax LeftParen { get; }
		public SeperatedSyntaxList<ExpressionSyntax> Parameters { get; }
		public TokenSyntax RightParen { get; }

		public FunctionCallSyntax(TokenSyntax identifier, TokenSyntax leftParen, SeperatedSyntaxList<ExpressionSyntax> parameters, TokenSyntax rightParen) {
			Identifier = identifier;
			LeftParen = leftParen;
			Parameters = parameters;
			RightParen = rightParen;
		}

		public override string ToString() {
			var builder = new StringBuilder();

			builder.Append(Identifier.ToString());
			builder.Append(LeftParen.ToString());
			builder.Append(Parameters.ToString());
			builder.Append(RightParen.ToString());

			return builder.ToString();
		}
	}
}
