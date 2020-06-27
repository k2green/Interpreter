using System;
using System.Runtime.Serialization;

namespace InterpreterLib {
	internal class ErrorEncounteredException : Exception {
		public ErrorEncounteredException() {
		}

		public ErrorEncounteredException(string message) : base(message) {
		}

		public ErrorEncounteredException(string message, Exception innerException) : base(message, innerException) {
		}

		protected ErrorEncounteredException(SerializationInfo info, StreamingContext context) : base(info, context) {
		}
	}
}