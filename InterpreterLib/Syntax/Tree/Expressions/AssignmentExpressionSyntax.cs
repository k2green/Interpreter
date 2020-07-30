using InterpreterLib.Syntax.Tree.Patterns;
using InterpreterLib.Syntax.Tree.Statements;
using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Syntax.Tree.Expressions {
	internal sealed class AssignmentExpressionSyntax : AccessorExpressionSyntax {
		public override SyntaxType Type => SyntaxType.Assignment;

		public override IEnumerable<SyntaxNode> Children {
			get {
				yield return IdentifierToken;
				yield return Definition;
				yield return OperatorToken;
				yield return Expression;
			}
		}

		public PatternSyntax IdentifierToken { get; }
		public TypeDefinitionSyntax Definition { get; }
		public TokenSyntax OperatorToken { get; }
		public ExpressionSyntax Expression { get; }
		public override TextLocation Location => IdentifierToken.Location;
		public override TextSpan Span => new TextSpan(IdentifierToken.Span.Start, Expression.Span.End);

		public AssignmentExpressionSyntax(PatternSyntax identifierToken, TypeDefinitionSyntax definition, TokenSyntax operatorToken, ExpressionSyntax expression) {
			IdentifierToken = identifierToken;
			Definition = definition;
			OperatorToken = operatorToken;
			Expression = expression;
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
