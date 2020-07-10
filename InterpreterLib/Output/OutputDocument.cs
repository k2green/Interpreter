using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Output {
	public class OutputDocument {

		public IEnumerable<IEnumerable<OutputFragment>> Lines { get; }

		internal OutputDocument(IEnumerable<IEnumerable<OutputFragment>> lines) {
			Lines = lines;
		}

		public void Output(Action<OutputFragment> outputFragment, Action outputNewline = null) {
			foreach (var line in Lines) {
				foreach (var fragment in line) {
					outputFragment(fragment);
				}

				if (outputNewline != null)
					outputNewline();
			}
		}
	}
}
