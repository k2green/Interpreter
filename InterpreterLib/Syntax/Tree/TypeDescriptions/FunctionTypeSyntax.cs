using InterpreterLib.Syntax.Tree.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Syntax.Tree.TypeDescriptions {
	internal sealed class FunctionTypeSyntax : TypeDescriptionSyntax {
		public override SyntaxType Type => SyntaxType.FunctionType;

		public override IEnumerable<SyntaxNode> Children {
			get {
				yield return ParameterSyntax;
				yield return DelimeterToken;
				yield return ReturnSyntax;
			}
		}

		public TupleTypeSyntax ParameterSyntax { get; }
		public TokenSyntax DelimeterToken { get; }
		public TypeDescriptionSyntax ReturnSyntax { get; }

		public FunctionTypeSyntax(TupleTypeSyntax parameterSyntax, TokenSyntax delimeterToken, TypeDescriptionSyntax returnSyntax) {
			ParameterSyntax = parameterSyntax;
			DelimeterToken = delimeterToken;
			ReturnSyntax = returnSyntax;
		}
	}
}
