using InterpreterLib.Binding.Tree.Expressions;
using InterpreterLib.Symbols.Types;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace InterpreterLib.Symbols.Binding {
	internal class BuiltInFunctions {

		public static FunctionSymbol Print = new FunctionSymbol("print", ImmutableArray.Create(new ParameterSymbol[] { new ParameterSymbol("inputString", ValueTypeSymbol.String) }), ValueTypeSymbol.Void, null);
		public static FunctionSymbol Input = new FunctionSymbol("input", ImmutableArray.Create(new ParameterSymbol[] { }), ValueTypeSymbol.String, null);

		public static IEnumerable<FunctionSymbol> GetAll() {
			return typeof(BuiltInFunctions).GetFields(BindingFlags.Public | BindingFlags.Static)
				.Where(f => f.FieldType == typeof(FunctionSymbol))
				.Select(f => (FunctionSymbol)f.GetValue(null));
		}
	}
}
