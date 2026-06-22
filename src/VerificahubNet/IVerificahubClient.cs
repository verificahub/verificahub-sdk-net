using System;
using System.Threading;
using System.Threading.Tasks;
using VerificahubNet.Models;
using VerificahubNet.Requests;

namespace VerificahubNet
{
    /// <summary>
    /// A client for the Verificahub external integrator API (<c>/v1/*</c>).
    /// <para>
    /// Successful (2xx) calls return the typed response. A non-2xx response throws
    /// <see cref="VerificahubApiException"/> (inspect its <see cref="VerificahubApiException.ErrorCode"/>);
    /// transport failures throw <see cref="VerificahubTransportException"/>, and unreadable responses
    /// throw <see cref="VerificahubProtocolException"/>.
    /// </para>
    /// </summary>
    public interface IVerificahubClient
    {
        /// <summary>Initiates a verification (<c>POST /v1/verify</c>).</summary>
        /// <param name="request">The verification parameters.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        Task<InitiateVerificationResponse> VerifyAsync(InitiateVerificationRequest request, CancellationToken cancellationToken = default);

        /// <summary>Gets the current status of a verification (<c>GET /v1/verify/{request_id}</c>).</summary>
        /// <param name="requestId">The verification identifier returned by <see cref="VerifyAsync"/>.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        Task<GetVerificationStatusResponse> GetStatusAsync(Guid requestId, CancellationToken cancellationToken = default);

        /// <summary>Submits a code entered by the user (<c>POST /v1/verify/check</c>).</summary>
        /// <param name="request">The verification identifier and the code.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        Task<CheckCodeResponse> CheckCodeAsync(CheckCodeRequest request, CancellationToken cancellationToken = default);

        /// <summary>Gets the account balance (<c>GET /v1/balance</c>).</summary>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        Task<GetBalanceResponse> GetBalanceAsync(CancellationToken cancellationToken = default);

        /// <summary>Gets a usage summary over a date range (<c>GET /v1/usage</c>).</summary>
        /// <param name="from">The first day to include (inclusive). Defaults server-side to 30 days ago.</param>
        /// <param name="to">The last day to include (inclusive). Defaults server-side to today.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        Task<GetUsageResponse> GetUsageAsync(DateTime? from = null, DateTime? to = null, CancellationToken cancellationToken = default);
    }
}
