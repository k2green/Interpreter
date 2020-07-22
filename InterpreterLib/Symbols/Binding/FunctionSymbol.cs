using InterpreterLib.Symbols.Types;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using System.Linq;
using InterpreterLib.Syntax.Tree.Global;

namespace InterpreterLib.Symbols.Binding {
	public sealed class FunctionSymbol : Symbol {

		public override SymbolType Type => SymbolType.Function;
		public override string Name { get; }
		public ImmutableArray<ParameterSymbol> Parameters { get; }
		public TypeSymbol ReturnType { get; }

		internal FunctionDeclarationSyntax FuncSyntax { get; }

		public LabelSymbol EndLabel => new LabelSymbol("FunctionEnd");

		internal FunctionSymbol(string name, ImmutableArray<ParameterSymbol> parameters, TypeSymbol returnType, FunctionDeclarationSyntax funcSyntax) {
			Name = name;
			Parameters = parameters;
			ReturnType = returnType;
			FuncSyntax = funcSyntax;
		}

		public override bool Equals(object obj) {
			if (!(obj is FunctionSymbol symbol)) return false;

			if (!Name.Equals(symbol.Name) || !ReturnType.Equals(symbol.ReturnType))
				return false;

			if (Parameters.Length != symbol.Parameters.Length)
				return false;

			for (int index = 0; index < Parameters.Length; index++) {
				if (!Parameters[index].Equals(symbol.Parameters[index]))
					return false;
			}

			return true;
		}

		private int ParametersHashCode => ((IStructuralEquatable)Parameters).GetHashCode(EqualityComparer<ParameterSymbol>.Default);

		public override int GetHashCode() {
			return HashCode.Combine(Name, ParametersHashCode, ReturnType);
		}

		public string GetParameterString() {
			var builder = new StringBuilder();

			for(int index = 0; index < Parameters.Length; index++ ) {
				builder.Append(Parameters[index]);

				if (index < Parameters.Length - 1)
					builder.Append(", ");
			}

			return builder.ToString();
		}

		public string GetGraphFileName() {
			var builder = new StringBuilder();
			var paramTypes = Parameters.Select(symbol => symbol.ValueType).ToImmutableArray();

			builder.Append(Name).Append(" ");

			if (paramTypes.Length > 0) {
				builder.Append("(");

				for (int index = 0; index < paramTypes.Length; index++) {
					builder.Append(paramTypes[index]);

					if (index < paramTypes.Length - 1)
						builder.Append(",");
				}

				builder.Append(") ");
			}

			builder.Append(ReturnType);

			return builder.ToString();
		}
	}
}
