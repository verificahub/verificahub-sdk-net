namespace VerificahubNet.Webhooks
{
    /// <summary>The <c>event</c> names carried by Verificahub <c>verification.*</c> webhooks.</summary>
    public static class WebhookEventTypes
    {
        /// <summary>The verification was initiated.</summary>
        public const string VerificationSent = "verification.sent";

        /// <summary>The challenge was delivered to the user.</summary>
        public const string VerificationDelivered = "verification.delivered";

        /// <summary>The user completed the verification successfully.</summary>
        public const string VerificationVerified = "verification.verified";

        /// <summary>The verification expired before completion.</summary>
        public const string VerificationExpired = "verification.expired";

        /// <summary>The verification failed.</summary>
        public const string VerificationFailed = "verification.failed";
    }
}
