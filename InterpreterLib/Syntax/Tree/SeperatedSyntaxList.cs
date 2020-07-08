using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace InterpreterLib.Syntax.Tree {
	internal class SeperatedSyntaxList<T> : IEnumerable<T> where T : SyntaxNode {
		private ImmutableArray<SyntaxNode> nodesAndSeperators;

		public TextSpan Span => new TextSpan(nodesAndSeperators.First().Span.Start, nodesAndSeperators.Last().Span.End);

		public int Count => (nodesAndSeperators.Length + 1) / 2;

		public T this[int index] => (T)nodesAndSeperators[index * 2];

		public SyntaxNode GetSeperator(int index) => nodesAndSeperators[index * 2 + 1];

		public SeperatedSyntaxList(ImmutableArray<SyntaxNode> nodes) {
			nodesAndSeperators = nodes;
		}

		public IEnumerator<T> GetEnumerator() {
			for (int i = 0; i < Count; i++)
				yield return this[i];
		}

		IEnumerator IEnumerable.GetEnumerator() {
			throw new NotImplementedException();
		}

		public override string ToString() {
			var builder = new StringBuilder();
			foreach(var node in nodesAndSeperators) {
				builder.Append(node.ToString());
			}

			return builder.ToString();
		}
	}
}
