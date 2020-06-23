using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib {
	public struct DiagnosticResult<T> {

		public IEnumerable<string> Diagnostics { get; }
		public T Value { get; }

		public DiagnosticResult(IEnumerable<string> diagnostics, T value) {
			Diagnostics = diagnostics;
			Value = value;
		}
	}
}
