using InterpreterLib.Symbols.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Binding.Tree {
	internal sealed class BoundTypeDefinition : BoundNode {
		public override NodeType Type => NodeType.TypeDefinition;

		public TypeSymbol ValueType { get; }

		public BoundTypeDefinition(ValueTypeSymbol valueType) {
			ValueType = valueType;
		}

		public override string ToString() => $": {ValueType.Name}";
	}
}
