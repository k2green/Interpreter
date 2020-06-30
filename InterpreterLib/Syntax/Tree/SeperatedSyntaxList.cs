using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Syntax.Tree {
	internal class SeperatedSyntaxList<T> : IEnumerable<T> where T : SyntaxNode {
		private List<SyntaxNode> nodesAndSeperators;

		public SeperatedSyntaxList(IEnumerable<SyntaxNode> nodes) {
			nodesAndSeperators = new List<SyntaxNode>();
			nodesAndSeperators.AddRange(nodes);
		}

		public int Count => (nodesAndSeperators.Count + 1) / 2;

		public T this[int index] => (T)nodesAndSeperators[index * 2];

		public SyntaxNode GetSeperator(int index) => nodesAndSeperators[index * 2 + 1];

		public IEnumerator<T> GetEnumerator() {
			for (int i = 0; i < Count; i++)
				yield return this[i];
		}

		IEnumerator IEnumerable.GetEnumerator() {
			throw new NotImplementedException();
		}
	}
}
