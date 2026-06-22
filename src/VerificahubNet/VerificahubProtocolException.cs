using System;

namespace VerificahubNet
{
    /// <summary>
    /// Thrown when a response was received but could not be interpreted — a malformed or empty body,
    /// or a non-2xx status without a parseable problem-details payload.
    /// </summary>
    public sealed class VerificahubProtocolException : VerificahubException
    {
        /// <summary>Initializes a new instance.</summary>
        /// <param name="message">A human-readable description of the failure.</param>
        /// <param name="statusCode">The HTTP status code of the response, if one was received.</param>
        /// <param name="responseBody">The raw response body (possibly truncated), if available.</param>
        /// <param name="innerException">The underlying exception, if any.</param>
        public VerificahubProtocolException(string message, int? statusCode, string? responseBody, Exception? innerException)
            : base(message, innerException)
        {
            StatusCode = statusCode;
            ResponseBody = responseBody;
        }

        /// <summary>The HTTP status code of the response, when one was received.</summary>
        public int? StatusCode { get; }

        /// <summary>The raw response body (possibly truncated), when available.</summary>
        public string? ResponseBody { get; }
    }
}
