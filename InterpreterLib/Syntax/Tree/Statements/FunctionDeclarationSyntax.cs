using InterpreterLib.Syntax.Tree.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Syntax.Tree.Statements {
	internal sealed class FunctionDeclarationSyntax : StatementSyntax {
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

		public override string ToString() {
			throw new NotImplementedException();
		}
	}
}
