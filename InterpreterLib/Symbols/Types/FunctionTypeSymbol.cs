using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace InterpreterLib.Symbols.Types {
	public sealed class FunctionTypeSymbol : TypeSymbol {
		public override SymbolType Type => SymbolType.FunctionType;

		public override string Name => ToString();

		public ImmutableArray<TypeSymbol> ParamTypes { get; }
		public TypeSymbol ReturnType { get; }

		public FunctionTypeSymbol(ImmutableArray<TypeSymbol> paramTypes, TypeSymbol returnType) {
			ParamTypes = paramTypes;
			ReturnType = returnType;
		}

		public override bool Equals(object obj) {
			if (!(obj is FunctionTypeSymbol symbol)) return false;

			if (!ReturnType.Equals(symbol.ReturnType) || ParamTypes.Length != symbol.ParamTypes.Length) return false;

			for (int index = 0; index < ParamTypes.Length; index++)
				if (!ParamTypes[index].Equals(symbol.ParamTypes[index]))
					return false;

			return true;
		}

		public override int GetHashCode() {
			var paramsCode = ((IStructuralEquatable)ParamTypes).GetHashCode(EqualityComparer<TypeSymbol>.Default);

			return HashCode.Combine(paramsCode, ReturnType);
		}

		public override string ToString() {
			var builder = new StringBuilder();

			builder.Append("(");

			for (int index = 0; index < ParamTypes.Length; index++) {
				builder.Append(ParamTypes[index]);

				if (index < ParamTypes.Length - 1)
					builder.Append(", ");
			}

			builder.Append(") => ").Append(ReturnType);

			return builder.ToString();
		}
	}
}
