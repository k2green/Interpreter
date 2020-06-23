using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib {
	public sealed class DiagnosticContainer : IEnumerable<string> {

		private List<IEnumerable<string>> diagnostics;

		public DiagnosticContainer() {
			diagnostics = new List<IEnumerable<string>>();
		}

		internal void AddDiagnostic(Diagnostic diagnostic) {
			diagnostics.Add(diagnostic);
		}

		internal void AddDiagnostic(IEnumerable<string> diagnostic) {
			diagnostics.Add(diagnostic);
		}

		public IEnumerator<string> GetEnumerator() {
			foreach (IEnumerable<string> diagnostic in diagnostics)
				foreach (string message in diagnostic)
					yield return message;
		}

		IEnumerator IEnumerable.GetEnumerator() {
			throw new NotImplementedException();
		}
	}
}
