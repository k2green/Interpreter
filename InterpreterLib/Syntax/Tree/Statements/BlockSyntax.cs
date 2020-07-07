using InterpreterLib.Syntax.Tree.Expressions;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace InterpreterLib.Syntax.Tree.Statements {
	internal sealed class BlockSyntax : StatementSyntax {
		public override SyntaxType Type => SyntaxType.Block;
		public override TextLocation Location => LeftBrace.Location;

		public override IEnumerable<SyntaxNode> Children {
			get {
				yield return LeftBrace;

				foreach (var statement in Statements)
					yield return statement;

				yield return RightBrace;
			}
		}

		public TokenSyntax LeftBrace { get; }
		public ImmutableArray<StatementSyntax> Statements;
		public TokenSyntax RightBrace { get; }

		public BlockSyntax(TokenSyntax leftBrace, ImmutableArray<StatementSyntax> statements, TokenSyntax rightBrace) {
			LeftBrace = leftBrace;
			Statements = statements;
			RightBrace = rightBrace;
		}
	}
}
