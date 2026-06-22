using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using VerificahubNet.Serialization;

namespace VerificahubNet.Webhooks
{
    /// <summary>
    /// Verifies the authenticity of Verificahub webhook deliveries. Each delivery carries an
    /// <c>X-Verificahub-Signature</c> header of the form <c>sha256=&lt;hex&gt;</c>, where the hex is
    /// the lowercase HMAC-SHA256 of the raw request body keyed by your account's <c>api_secret</c>.
    /// </summary>
    public sealed class WebhookSignatureValidator
    {
        /// <summary>The HTTP header that carries the delivery signature.</summary>
        public const string SignatureHeaderName = "X-Verificahub-Signature";

        private readonly byte[] _secretKey;

        /// <summary>Creates a validator keyed by your account's API secret.</summary>
        /// <param name="apiSecret">The account's <c>api_secret</c> (the same value used for Basic auth).</param>
        public WebhookSignatureValidator(string apiSecret)
        {
            if (string.IsNullOrEmpty(apiSecret))
            {
                throw new ArgumentException("API secret must not be null or empty.", nameof(apiSecret));
            }

            _secretKey = Encoding.UTF8.GetBytes(apiSecret);
        }

        /// <summary>Computes the expected signature value (including the <c>sha256=</c> prefix) for a body.</summary>
        /// <param name="rawBody">The raw POST body, exactly as received.</param>
        /// <returns>The signature in the form <c>sha256=&lt;lowercase-hex&gt;</c>.</returns>
        public string ComputeSignature(string rawBody)
        {
            if (rawBody == null)
            {
                throw new ArgumentNullException(nameof(rawBody));
            }

            using (var hmac = new HMACSHA256(_secretKey))
            {
                byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(rawBody));
                return "sha256=" + ToHex(hash);
            }
        }

        /// <summary>Verifies a delivery's signature using a constant-time comparison.</summary>
        /// <param name="rawBody">The raw POST body, exactly as received.</param>
        /// <param name="signatureHeader">The value of the <see cref="SignatureHeaderName"/> header.</param>
        /// <returns><c>true</c> if the signature is valid; otherwise <c>false</c>.</returns>
        public bool IsValid(string rawBody, string? signatureHeader)
        {
            if (string.IsNullOrEmpty(signatureHeader))
            {
                return false;
            }

            string expected = ComputeSignature(rawBody);
            return FixedTimeEquals(expected, signatureHeader!);
        }

        /// <summary>Verifies a delivery and, if authentic, deserializes its payload.</summary>
        /// <param name="rawBody">The raw POST body, exactly as received.</param>
        /// <param name="signatureHeader">The value of the <see cref="SignatureHeaderName"/> header.</param>
        /// <param name="webhookEvent">The parsed event when the signature is valid and the body parses; otherwise <c>null</c>.</param>
        /// <returns><c>true</c> if the signature is valid and the body was parsed; otherwise <c>false</c>.</returns>
        public bool TryParse(string rawBody, string? signatureHeader, out WebhookEvent? webhookEvent)
        {
            webhookEvent = null;
            if (!IsValid(rawBody, signatureHeader))
            {
                return false;
            }

            try
            {
                webhookEvent = JsonSerializer.Deserialize<WebhookEvent>(rawBody, VerificahubJson.Options);
            }
            catch (JsonException)
            {
                return false;
            }

            return webhookEvent != null;
        }

        private static string ToHex(byte[] bytes)
        {
            var sb = new StringBuilder(bytes.Length * 2);
            foreach (byte b in bytes)
            {
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        }

        private static bool FixedTimeEquals(string a, string b)
        {
            // Constant-time relative to the expected length; case-insensitive on the hex digest.
            if (a.Length != b.Length)
            {
                return false;
            }

            int diff = 0;
            for (int i = 0; i < a.Length; i++)
            {
                diff |= char.ToLowerInvariant(a[i]) ^ char.ToLowerInvariant(b[i]);
            }
            return diff == 0;
        }
    }
}
