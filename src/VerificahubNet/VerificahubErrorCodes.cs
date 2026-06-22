namespace VerificahubNet
{
    /// <summary>
    /// The stable machine-readable <c>error_code</c> values returned in problem-details responses on
    /// the external API. Compare <see cref="VerificahubApiException.ErrorCode"/> against these.
    /// </summary>
    public static class VerificahubErrorCodes
    {
        /// <summary>The request was malformed or contained invalid fields (HTTP 400).</summary>
        public const string ValidationError = "validation_error";

        /// <summary>The phone number is outside the supported region (HTTP 400).</summary>
        public const string RegionNotSupported = "region_not_supported";

        /// <summary>The submitted code was wrong; see <see cref="VerificahubApiException.AttemptsRemaining"/> (HTTP 400).</summary>
        public const string InvalidCode = "invalid_code";

        /// <summary>The account balance is too low to initiate the verification (HTTP 402).</summary>
        public const string InsufficientBalance = "insufficient_balance";

        /// <summary>No gateway number was available to service the verification (HTTP 402).</summary>
        public const string NoGatewayAvailable = "no_gateway_available";

        /// <summary>Pricing could not be resolved for the verification (HTTP 402).</summary>
        public const string PricingUnavailable = "pricing_unavailable";

        /// <summary>The verification or account was not found (HTTP 404).</summary>
        public const string NotFound = "not_found";

        /// <summary>The verification is not in a state that accepts this operation (HTTP 409).</summary>
        public const string NotPending = "not_pending";

        /// <summary>The verification could not be delivered; no charge applies (HTTP 422).</summary>
        public const string DeliveryUnavailable = "delivery_unavailable";

        /// <summary>An upstream verification provider failed; no charge applies (HTTP 502).</summary>
        public const string ProviderError = "provider_error";

        /// <summary>Too many requests; retry after the <c>Retry-After</c> interval (HTTP 429).</summary>
        public const string RateLimitExceeded = "rate_limit_exceeded";

        /// <summary>Authentication failed or was missing (HTTP 401).</summary>
        public const string Unauthorized = "unauthorized";
    }
}
