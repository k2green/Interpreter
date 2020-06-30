using InterpreterLib.Syntax.Tree.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Syntax.Tree.Statements {
	internal sealed class BlockSyntax : StatementSyntax {
		public override SyntaxType Type => SyntaxType.Block;
		public override IEnumerable<SyntaxNode> Children { get; }
		public override TextSpan Span { get; }

		private List<StatementSyntax> statementsList;

		public TokenSyntax LeftBrace { get; }
		public IReadOnlyList<StatementSyntax> Statements => statementsList;
		public TokenSyntax RightBrace { get; }

		public BlockSyntax(TokenSyntax leftBrace, IEnumerable<StatementSyntax> statements, TokenSyntax rightBrace) {
			LeftBrace = leftBrace;

			statementsList = new List<StatementSyntax>();
			statementsList.AddRange(statements);

			RightBrace = rightBrace;

			Span = CreateNewSpan(leftBrace.Span, rightBrace.Span);

			var childList = new List<SyntaxNode>();
			childList.Add(leftBrace);
			childList.AddRange(statements);
			childList.Add(rightBrace);

			Children = childList;
		}

		public override string ToString() {
			throw new NotImplementedException();
		}
	}
}
