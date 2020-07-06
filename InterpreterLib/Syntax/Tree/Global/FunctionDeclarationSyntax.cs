using InterpreterLib.Syntax.Tree.Expressions;
using InterpreterLib.Syntax.Tree.Statements;
using System.Collections.Generic;

namespace InterpreterLib.Syntax.Tree.Global {
	internal sealed class FunctionDeclarationSyntax : GlobalSyntax {
		public override SyntaxType Type => SyntaxType.FunctionDeclaration;

		public override IEnumerable<SyntaxNode> Children {
			get {
				yield return KeywToken;
				yield return Identifier;
				yield return Parameters;
				yield return ReturnType;
				yield return Body;
			}
		}

		public override TextSpan Span { get; }

		public TokenSyntax KeywToken { get; }
		public TokenSyntax Identifier { get; }
		public ParameterDefinitionSyntax Parameters { get; }
		public TypeDefinitionSyntax ReturnType { get; }
		public StatementSyntax Body { get; }

		public FunctionDeclarationSyntax(TokenSyntax keywToken, TokenSyntax identifier, ParameterDefinitionSyntax parameters, TypeDefinitionSyntax returnType, StatementSyntax body) {
			KeywToken = keywToken;
			Identifier = identifier;
			Parameters = parameters;
			ReturnType = returnType;
			Body = body;

			Span = CreateNewSpan(keywToken.Span, body.Span);
		}
	}
}
