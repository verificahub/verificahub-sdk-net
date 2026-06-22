using System;

namespace VerificahubNet
{
    /// <summary>The base type for all exceptions thrown by the Verificahub SDK.</summary>
    public abstract class VerificahubException : Exception
    {
        /// <summary>Initializes a new instance with a message and optional inner exception.</summary>
        /// <param name="message">A human-readable description of the failure.</param>
        /// <param name="innerException">The underlying exception, if any.</param>
        protected VerificahubException(string message, Exception? innerException)
            : base(message, innerException)
        {
        }
    }
}
