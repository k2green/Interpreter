using InterpreterLib.Binding.Types;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace InterpreterLib.Binding.Tree.Statements {
	internal sealed class BoundFunctionCall : BoundStatement {
		public override NodeType Type => NodeType.FunctionCall;

		public IEnumerable<ParameterSymbol> Parameters { get; }
		public BoundStatement Statement { get; }

		public BoundFunctionCall(IEnumerable<ParameterSymbol> parameters, BoundStatement statement) {
			Parameters = parameters;
			Statement = statement;
		}
	}
}
