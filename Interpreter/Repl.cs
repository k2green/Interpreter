using InterpreterLib.Binding.Types;
using InterpreterLib.Runtime;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Interpreter {
	public abstract class Repl {

		protected RuntimeParser parser;
		protected bool showTree;
		protected bool showProgram;

		protected Dictionary<VariableSymbol, object> variables;
		protected BindingEnvironment environment;

		public void Run() {
			variables = new Dictionary<VariableSymbol, object>();

			while (true) {
				string input = EditSubmission();

				if (input == null)
					return;

				EvaluateInput(input);
			}
		}

		private string EditSubmission() {
			var observable = new ObservableCollection<string>();
			var view = new SubmissionView(observable);

			while(true) {
				var key = Console.ReadKey(true);
				HandleKey(key, observable, view);
			}
		}

		private void HandleKey(ConsoleKeyInfo key, ObservableCollection<string> observable, SubmissionView view) {
			switch(key.Key) {
				case ConsoleKey.LeftArrow:
					HandleLeftArrow(observable, view);
					return;
				case ConsoleKey.RightArrow:
					HandleRightArrow(observable, view);
					return;
				case ConsoleKey.UpArrow:
					HandleUpArrow(observable, view);
					return;
				case ConsoleKey.DownArrow:
					HandleDownArrow(observable, view);
					return;
				case ConsoleKey.Enter:
					HandleEnter(observable, view);
					return;
			}

			if(key.KeyChar >= ' ') {
				HandleTyping(observable, view, key.KeyChar.ToString());
			}
		}

		private void HandleTyping(ObservableCollection<string> observable, SubmissionView view, string text) {
			observable[view.CursorLine] = observable[view.CursorLine].Insert(view.CursorCharacter, text);
			view.CursorCharacter++;
		}

		private void HandleEnter(ObservableCollection<string> observable, SubmissionView view) {
			if()
		}

		private void HandleDownArrow(ObservableCollection<string> observable, SubmissionView view) {
			if (view.CursorLine < observable.Count - 1)
				view.CursorLine++;
		}

		private void HandleUpArrow(ObservableCollection<string> observable, SubmissionView view) {
			if (view.CursorLine > 0)
				view.CursorLine--;
		}

		private void HandleRightArrow(ObservableCollection<string> observable, SubmissionView view) {
			var line = observable[view.CursorLine];
			if (view.CursorCharacter < line.Length - 1)
				view.CursorCharacter++;
		}

		private void HandleLeftArrow(ObservableCollection<string> observable, SubmissionView view) {
			if (view.CursorCharacter > 0)
				view.CursorCharacter--;
		}

		protected abstract void EvaluateInput(string input);

		protected virtual bool IsCommand(string input) {
			if (!input.StartsWith('#'))
				return false;

			return true;
		}

		protected bool EvaluateCommand(string command) {
			switch (command.ToLower()) {
				case "clear":
					Console.Clear();
					return true;
				case "reset":
					variables = new Dictionary<VariableSymbol, object>();
					environment = null;
					return true;
				case "showtree":
					showTree = !showTree;
					Console.WriteLine($"Showing tree: {showTree}");
					return true;
				case "showprogram":
					showProgram = !showProgram;
					Console.WriteLine($"Showing program: {showProgram}");
					return true;
				default:
					return false;
			}
		}

		protected virtual bool IsCompleteSubmission(string input) {
			if (string.IsNullOrEmpty(input))
				return false;

			return true;
		}
	}
}
