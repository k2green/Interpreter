using InterpreterLib.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Binding.Tree.Expressions {
	internal sealed class BoundTypeDefinition : BoundNode {
		public override NodeType Type => NodeType.TypeDefinition;

		public TypeSymbol ValueType { get; }

		public BoundTypeDefinition(TypeSymbol valueType) {
			ValueType = valueType;
		}

		public override string ToString() => $": {ValueType.Name}";
	}
}
