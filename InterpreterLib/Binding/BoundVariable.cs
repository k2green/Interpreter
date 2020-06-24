using InterpreterLib.Binding.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Binding {
	public struct BoundVariable {

		public string Name { get; }
		public bool IsReadOnly { get; }
		public BoundType ValueType { get; }

		public BoundVariable(string name, bool isReadOnly, BoundType valueType) {
			Name = name;
			IsReadOnly = isReadOnly;
			ValueType = valueType;
		}

		public override string ToString() => $"{Name} : {ValueType}";
	}
}
