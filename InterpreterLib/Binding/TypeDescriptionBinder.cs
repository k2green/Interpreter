using InterpreterLib.Symbols.Types;
using InterpreterLib.Syntax.Tree;
using InterpreterLib.Syntax.Tree.TypeDescriptions;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace InterpreterLib.Binding {
	internal static class TypeDescriptionBinder {
		internal static TypeSymbol BindTypeDescription(SyntaxNode syntax, bool isReadOnly = false) {
			switch (syntax.Type) {
				case SyntaxType.ValueType:
					return BindValueType((ValueTypeSyntax)syntax, isReadOnly);
				case SyntaxType.TupleType:
					return BindTupleType((TupleTypeSyntax)syntax, isReadOnly);
				case SyntaxType.FunctionType:
					return BindFunctionType((FunctionTypeSyntax)syntax, isReadOnly);
				default: return null;
			}
		}

		private static TypeSymbol BindFunctionType(FunctionTypeSyntax syntax, bool isReadOnly) {
			var tupleBind = BindTupleType(syntax.ParameterSyntax, isReadOnly);
			var returnBind = BindTypeDescription(syntax.ReturnSyntax, isReadOnly);

			if (returnBind == null)
				return null;

			if (tupleBind == null)
				return new FunctionTypeSymbol(ImmutableArray.Create(new TypeSymbol[] { }), returnBind);

			return new FunctionTypeSymbol(tupleBind.Types, returnBind);
		}

		private static TupleSymbol BindTupleType(TupleTypeSyntax syntax, bool isReadOnly) {
			if (syntax == null || syntax.Types == null || syntax.Types.Count <= 0)
				return null;

			var builder = ImmutableArray.CreateBuilder<TypeSymbol>();

			foreach (var type in syntax.Types) {
				var boundType = BindTypeDescription(type, isReadOnly);

				if (boundType == null)
					return null;

				builder.Add(boundType);
			}

			return new TupleSymbol(builder.ToImmutable());
		}

		private static TypeSymbol BindValueType(ValueTypeSyntax syntax, bool isReadOnly) {
			return ValueTypeSymbol.FromString(syntax.TypeName.ToString());
		}
	}
}
