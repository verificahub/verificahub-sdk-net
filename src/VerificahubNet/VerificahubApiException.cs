using VerificahubNet.Models;

namespace VerificahubNet
{
    /// <summary>
    /// Thrown when the Verificahub API returns a non-2xx response carrying an RFC-7807
    /// problem-details body. Inspect <see cref="ErrorCode"/> (see <see cref="VerificahubErrorCodes"/>)
    /// to branch on the specific failure.
    /// </summary>
    public sealed class VerificahubApiException : VerificahubException
    {
        /// <summary>Initializes a new instance from a parsed problem-details response.</summary>
        /// <param name="statusCode">The HTTP status code.</param>
        /// <param name="errorCode">The stable machine-readable error code.</param>
        /// <param name="detail">The human-readable detail message, if any.</param>
        /// <param name="traceId">The correlation/trace identifier, if any.</param>
        /// <param name="attemptsRemaining">For <c>invalid_code</c>, the number of attempts remaining.</param>
        /// <param name="responseBody">The raw response body, when available.</param>
        /// <param name="problem">The parsed problem-details payload, when available.</param>
        public VerificahubApiException(
            int statusCode,
            string errorCode,
            string? detail,
            string? traceId,
            int? attemptsRemaining,
            string? responseBody,
            ProblemDetails? problem)
            : base(BuildMessage(statusCode, errorCode, detail), null)
        {
            StatusCode = statusCode;
            ErrorCode = errorCode;
            Detail = detail;
            TraceId = traceId;
            AttemptsRemaining = attemptsRemaining;
            ResponseBody = responseBody;
            Problem = problem;
        }

        /// <summary>The HTTP status code of the response.</summary>
        public int StatusCode { get; }

        /// <summary>The stable machine-readable error code (for example <c>insufficient_balance</c>).</summary>
        public string ErrorCode { get; }

        /// <summary>The human-readable detail message, if the API supplied one.</summary>
        public string? Detail { get; }

        /// <summary>The correlation/trace identifier, useful when contacting support.</summary>
        public string? TraceId { get; }

        /// <summary>
        /// For an <see cref="VerificahubErrorCodes.InvalidCode"/> error, the number of code-check
        /// attempts the user has left before the verification fails; otherwise <c>null</c>.
        /// </summary>
        public int? AttemptsRemaining { get; }

        /// <summary>The raw response body, when it was read.</summary>
        public string? ResponseBody { get; }

        /// <summary>The parsed problem-details payload, when available.</summary>
        public ProblemDetails? Problem { get; }

        private static string BuildMessage(int statusCode, string errorCode, string? detail)
            => string.IsNullOrEmpty(detail)
                ? $"Verificahub API error (HTTP {statusCode}, {errorCode})."
                : $"Verificahub API error (HTTP {statusCode}, {errorCode}): {detail}";
    }
}
