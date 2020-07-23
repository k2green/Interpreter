using InterpreterLib.Syntax.Tree.Expressions;
using InterpreterLib.Syntax.Tree.Statements;
using System.Collections.Generic;

namespace InterpreterLib.Syntax.Tree.Expressions {
	internal sealed class FunctionDeclarationSyntax : ExpressionSyntax {
		public override SyntaxType Type => SyntaxType.FunctionDeclaration;

		public override IEnumerable<SyntaxNode> Children {
			get {
				yield return KeywToken;
				yield return Identifier;

				foreach (var param in Parameters)
					yield return param;

				yield return ReturnType;
				yield return Body;
			}
		}

		public override TextLocation Location => KeywToken.Location;
		public override TextSpan Span => new TextSpan(KeywToken.Span.Start, Body.Span.End);

		public TokenSyntax KeywToken { get; }
		public TokenSyntax Identifier { get; }
		public TokenSyntax LeftParenthesis { get; }
		public SeperatedSyntaxList<TypedIdentifierSyntax> Parameters { get; }
		public TokenSyntax RightParenthesis { get; }
		public TypeDefinitionSyntax ReturnType { get; }
		public StatementSyntax Body { get; }

		public string ImplicitLabel { get; }

		public FunctionDeclarationSyntax(
			TokenSyntax keywToken,
			TokenSyntax identifier,
			TokenSyntax leftParenthesis,
			SeperatedSyntaxList<TypedIdentifierSyntax> parameters,
			TokenSyntax rightParenthesis,
			TypeDefinitionSyntax returnType,
			StatementSyntax body,
			string implicitLabel
			) {
			KeywToken = keywToken;
			Identifier = identifier;
			LeftParenthesis = leftParenthesis;
			Parameters = parameters;
			RightParenthesis = rightParenthesis;
			ReturnType = returnType;
			Body = body;
			ImplicitLabel = implicitLabel;
		}
	}
}
