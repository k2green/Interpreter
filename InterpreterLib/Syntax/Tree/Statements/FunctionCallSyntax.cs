using InterpreterLib.Syntax.Tree.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Syntax.Tree.Statements {
	internal sealed class FunctionCallSyntax : StatementSyntax {
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

		public override TextSpan Span { get; }
		public TokenSyntax Identifier { get; }
		public TokenSyntax LeftParen { get; }
		public SeperatedSyntaxList<ExpressionSyntax> Parameters { get; }
		public TokenSyntax RightParen { get; }

		public FunctionCallSyntax(TokenSyntax identifier, TokenSyntax leftParen, SeperatedSyntaxList<ExpressionSyntax> parameters, TokenSyntax rightParen) {
			Identifier = identifier;
			LeftParen = leftParen;
			Parameters = parameters;
			RightParen = rightParen;

			Span = CreateNewSpan(identifier.Span, rightParen.Span);
		}

		public override string ToString() {
			throw new NotImplementedException();
		}
	}
}
