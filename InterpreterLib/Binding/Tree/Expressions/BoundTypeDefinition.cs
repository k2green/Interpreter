using InterpreterLib.Symbols.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Binding.Tree.Expressions {
	internal sealed class BoundTypeDefinition : BoundNode {
		public override NodeType Type => NodeType.TypeDefinition;

		public ValueTypeSymbol ValueType { get; }

		public BoundTypeDefinition(ValueTypeSymbol valueType) {
			ValueType = valueType;
		}

		public override string ToString() => $": {ValueType.Name}";
	}
}
