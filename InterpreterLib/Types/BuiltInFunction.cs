using InterpreterLib.Binding.Tree.Expressions;
using InterpreterLib.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace InterpreterLib.Binding {
	internal class BuiltInFunctions {

		public static FunctionSymbol Print = new FunctionSymbol("print", new List<ParameterSymbol>() { new ParameterSymbol("inputString", TypeSymbol.String) }, TypeSymbol.Void);
		public static FunctionSymbol Input = new FunctionSymbol("input", new List<ParameterSymbol>() { }, TypeSymbol.String);

		public static IEnumerable<FunctionSymbol> GetAll() {
			return typeof(BuiltInFunctions).GetFields(BindingFlags.Public | BindingFlags.Static)
				.Where(f => f.FieldType == typeof(FunctionSymbol))
				.Select(f => (FunctionSymbol)f.GetValue(null));
		}
	}
}
