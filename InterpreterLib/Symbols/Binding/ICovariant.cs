using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Symbols.Binding {
	public interface ICovariant<out T> {
		T ValueType { get; }
	}
}
