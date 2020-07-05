using InterpreterLib.Syntax.Tree.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Syntax.Tree.Statements {
	internal sealed class BlockSyntax : StatementSyntax {
		public override SyntaxType Type => SyntaxType.Block;
		public override TextSpan Span { get; }

		public override IEnumerable<SyntaxNode> Children {
			get {
				yield return LeftBrace;

				foreach (var statement in Statements)
					yield return statement;

				yield return RightBrace;
			}
		}

		public TokenSyntax LeftBrace { get; }
		public IEnumerable<StatementSyntax> Statements;
		public TokenSyntax RightBrace { get; }

		public BlockSyntax(TokenSyntax leftBrace, IEnumerable<StatementSyntax> statements, TokenSyntax rightBrace) {
			LeftBrace = leftBrace;
			Statements = statements;
			RightBrace = rightBrace;

			Span = CreateNewSpan(leftBrace.Span, rightBrace.Span);
		}
	}
}
