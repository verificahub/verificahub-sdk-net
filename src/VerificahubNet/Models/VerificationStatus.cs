using System.Text.Json.Serialization;
using VerificahubNet.Serialization;

namespace VerificahubNet.Models
{
    /// <summary>The lifecycle state of a verification.</summary>
    [JsonConverter(typeof(SnakeCaseEnumConverter<VerificationStatus>))]
    public enum VerificationStatus
    {
        /// <summary>An unrecognized or unset status (forward-compatibility fallback).</summary>
        Unknown = 0,

        /// <summary>The verification has been initiated.</summary>
        Sent,

        /// <summary>The challenge was delivered to the user.</summary>
        Delivered,

        /// <summary>The user completed the verification successfully.</summary>
        Verified,

        /// <summary>The verification expired before completion.</summary>
        Expired,

        /// <summary>The verification failed.</summary>
        Failed
    }
}
