using InterpreterLib.Binding;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace InterpreterLib.Output {
	public abstract class OutputBase<T> {

		public T OutputItem { get; }
		protected OutputDocumentBuilder builder;

		public OutputDocument Document => builder.ToDocument();

		public static readonly string DelimeterString = ": ";

		public static readonly string IndentString = new string(' ', 4);

		public static readonly Color DefaultColour = Color.White;

		public static readonly Color LabelColour = Color.Gray;
		public static readonly Color BranchColour = Color.Cyan;

		public static readonly Color TypeColour = Color.Orange;

		public static readonly Color FunctionNameColour = Color.DodgerBlue;
		public static readonly Color VariableColour = Color.Yellow;

		public static readonly Color NumberColour = Color.Aquamarine;
		public static readonly Color StringColour = Color.BurlyWood;
		public static readonly Color BooleanColour = Color.MediumVioletRed;
		public static readonly Color StatementColour = Color.Cyan;

		public OutputBase(T item) {
			OutputItem = item;
			builder = new OutputDocumentBuilder();
		}
	}
}
