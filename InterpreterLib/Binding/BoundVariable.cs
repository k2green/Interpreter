using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Binding {
	public struct BoundVariable {

		public string Name { get; }
		public bool IsReadOnly { get; }
		public Type ValueType { get; }

		public BoundVariable(string name, bool isReadOnly, Type valueType) {
			Name = name;
			IsReadOnly = isReadOnly;
			ValueType = valueType;
		}
	}
}
