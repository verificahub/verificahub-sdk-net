using System;
using System.Text.Json.Serialization;

namespace VerificahubNet.Requests
{
    /// <summary>Parameters for submitting a verification code (<c>POST /v1/verify/check</c>).</summary>
    public sealed class CheckCodeRequest
    {
        /// <summary>Creates a request for the given verification and code.</summary>
        /// <param name="requestId">The verification identifier returned by <c>VerifyAsync</c>.</param>
        /// <param name="code">The code the user entered (4–8 digits).</param>
        public CheckCodeRequest(Guid requestId, string code)
        {
            RequestId = requestId;
            Code = code;
        }

        /// <summary>The verification identifier.</summary>
        [JsonPropertyName("request_id")]
        public Guid RequestId { get; set; }

        /// <summary>The code the user entered (4–8 digits).</summary>
        [JsonPropertyName("code")]
        public string Code { get; set; }
    }
}
