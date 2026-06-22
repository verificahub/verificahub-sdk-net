using System;

namespace VerificahubNet
{
    /// <summary>
    /// Thrown when a request could not reach the Verificahub API or no response was received —
    /// a network, DNS, TLS, or timeout failure. The request may or may not have been processed,
    /// so reconcile with <c>GetStatusAsync</c> when the outcome matters.
    /// </summary>
    public sealed class VerificahubTransportException : VerificahubException
    {
        /// <summary>Initializes a new instance with a message and the underlying exception.</summary>
        /// <param name="message">A human-readable description of the failure.</param>
        /// <param name="innerException">The underlying transport exception, if any.</param>
        public VerificahubTransportException(string message, Exception? innerException)
            : base(message, innerException)
        {
        }
    }
}
