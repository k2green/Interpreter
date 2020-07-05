using InterpreterLib.Binding.Tree.Expressions;
using InterpreterLib.Binding.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Binding {
	internal class BaseFunction {

		internal static FunctionSymbol Print = new FunctionSymbol("print", new List<ParameterSymbol>() { new ParameterSymbol("inputString", TypeSymbol.String) }, TypeSymbol.Void);
		internal static FunctionSymbol Input = new FunctionSymbol("input", new List<ParameterSymbol>() { }, TypeSymbol.String);

		private static Dictionary<string, FunctionSymbol> symbols;
		private static Dictionary<string, FunctionSymbol> Symbols {
			get {
				if (symbols == null) {
					symbols = new Dictionary<string, FunctionSymbol>();

					symbols.Add(Print.Name, Print);
					symbols.Add(Input.Name, Input);
				}

				return symbols;
			}
		}

		public static bool TryFindSymbol(string name, out FunctionSymbol symbol) => Symbols.TryGetValue(name, out symbol);
	}
}
