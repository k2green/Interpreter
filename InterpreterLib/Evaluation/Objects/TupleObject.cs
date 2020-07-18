using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using InterpreterLib.Binding.Tree.Expressions;
using InterpreterLib.Symbols.Binding;
using InterpreterLib.Symbols.Types;

namespace InterpreterLib.Evaluation.Objects {
	internal class TupleObject : RuntimeObject, ScopedObject {
		public Dictionary<VariableSymbol, object> Variables { get; }
		public ImmutableArray<VariableSymbol> VariableOrder { get; }

		public TupleObject(Dictionary<VariableSymbol, object> variables, ImmutableArray<VariableSymbol> variableOrder) {
			Variables = variables;
			VariableOrder = variableOrder;
		}

		public override string ToString() {
			var builder = new StringBuilder();

			builder.Append("(");

			for(int index = 0; index < VariableOrder.Length; index++) {
				builder.Append(Variables[VariableOrder[index]]);

				if(index < VariableOrder.Length - 1)
					builder.Append(", ");
			}

			builder.Append("): (");

			for (int index = 0; index < VariableOrder.Length; index++) {
				builder.Append(VariableOrder[index].ValueType);

				if (index < VariableOrder.Length - 1)
					builder.Append(", ");
			}

			return builder.Append(")").ToString();
		}
	}
}
