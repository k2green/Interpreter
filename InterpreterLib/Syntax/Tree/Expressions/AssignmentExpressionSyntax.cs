using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Syntax.Tree.Expressions {
	internal sealed class AssignmentExpressionSyntax : ExpressionSyntax {
		public override SyntaxType Type => SyntaxType.Assignment;

		public override IEnumerable<SyntaxNode> Children {
			get {
				yield return IdentifierToken;
				yield return Definition;
				yield return OperatorToken;
				yield return Expression;
			}
		}

		public TokenSyntax IdentifierToken { get; }
		public TypeDefinitionSyntax Definition { get; }
		public TokenSyntax OperatorToken { get; }
		public ExpressionSyntax Expression { get; }
		public override TextLocation Span { get; }

		public AssignmentExpressionSyntax( TokenSyntax identifierToken, TypeDefinitionSyntax definition, TokenSyntax operatorToken, ExpressionSyntax expression) {
			IdentifierToken = identifierToken;
			Definition = definition;
			OperatorToken = operatorToken;
			Expression = expression;

			Span = CreateNewSpan(identifierToken.Span, expression.Span);
		}

		public override string ToString() {
			var builder = new StringBuilder();

			builder.Append(IdentifierToken).Append(" ");
			builder.Append(Definition).Append(" ");
			builder.Append(OperatorToken).Append(" ");
			builder.Append(Expression);

			return builder.ToString();
		}
	}
}
