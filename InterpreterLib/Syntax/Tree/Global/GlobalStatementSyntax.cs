using InterpreterLib.Syntax.Tree.Statements;
using System;
using System.Collections.Generic;

namespace InterpreterLib.Syntax.Tree.Global {
	internal sealed class GlobalStatementSyntax : GlobalSyntax {
		public override SyntaxType Type => SyntaxType.GlobalStatement;

		public override IEnumerable<SyntaxNode> Children {
			get {
				yield return Statement;
			}
		}

		public override TextLocation Location => Statement.Location;

		public StatementSyntax Statement { get; }

		public GlobalStatementSyntax(StatementSyntax statement) {
			Statement = statement;
		}
	}
}
