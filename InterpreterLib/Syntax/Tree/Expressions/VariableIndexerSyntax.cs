using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Syntax.Tree.Expressions {
	internal sealed class VariableIndexerSyntax : AccessorExpressionSyntax {
		public override SyntaxType Type => SyntaxType.VariableIndexer;
		public override TextLocation Location => Identifier.Location;
		public override TextSpan Span => new TextSpan(Identifier.Span.Start, RightBracket.Span.End);

		public override IEnumerable<SyntaxNode> Children {
			get {
				yield return Identifier;
				yield return LeftBracket;
				yield return Expression;
				yield return RightBracket;
			}
		}

		public TokenSyntax Identifier { get; }
		public TokenSyntax LeftBracket { get; }
		public ExpressionSyntax Expression { get; }
		public TokenSyntax RightBracket { get; }

		public VariableIndexerSyntax(TokenSyntax identifier, TokenSyntax leftBracket, ExpressionSyntax expression, TokenSyntax rightBracket) {
			Identifier = identifier;
			LeftBracket = leftBracket;
			Expression = expression;
			RightBracket = rightBracket;
		}
	}
}
