using InterpreterLib.Binding.Tree.Expressions;
using InterpreterLib.Types;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace InterpreterLib.Binding {
	internal class BuiltInFunctions {

		public static FunctionSymbol Print = new FunctionSymbol("print", ImmutableArray.Create(new ParameterSymbol[] { new ParameterSymbol("inputString", TypeSymbol.String) }), TypeSymbol.Void);
		public static FunctionSymbol Input = new FunctionSymbol("input", ImmutableArray.Create(new ParameterSymbol[] { }), TypeSymbol.String);

		public static IEnumerable<FunctionSymbol> GetAll() {
			return typeof(BuiltInFunctions).GetFields(BindingFlags.Public | BindingFlags.Static)
				.Where(f => f.FieldType == typeof(FunctionSymbol))
				.Select(f => (FunctionSymbol)f.GetValue(null));
		}
	}
}
