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

        /// <summary>Lists the live per-method verification prices for your account currency (<c>GET /v1/prices</c>).</summary>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        Task<GetPricesResponse> GetPricesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Looks up operator, region and porting for a number (<c>POST /v1/lookup/number-info</c>).
        /// </summary>
        /// <remarks>
        /// <b>Not a verification.</b> It reads the operator's registry and proves nothing about who
        /// holds the phone — there is no session, no code and no webhook. Useful before you spend on
        /// a verification: the operator a number is on now is not what its prefix implies, because a
        /// ported number keeps the prefix of the operator that issued it.
        /// <para>
        /// A number the registry knows nothing about returns <see cref="LookupStatus.NoData"/> with
        /// a zero cost — an answer, not an error.
        /// </para>
        /// </remarks>
        /// <param name="request">The number to look up.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        Task<LookupResponse> LookupNumberInfoAsync(LookupRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Scores how active a number is in the network, 0 to 1 (<c>POST /v1/lookup/activity</c>).
        /// </summary>
        /// <remarks>
        /// Intended for cleaning a list before a send, not for refusing an individual customer: the
        /// same number can score differently on repeated lookups. As with number info, no data is an
        /// answer and costs nothing.
        /// </remarks>
        /// <param name="request">The number to look up.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        Task<LookupResponse> LookupActivityAsync(LookupRequest request, CancellationToken cancellationToken = default);

        /// <summary>Gets a usage summary over a date range (<c>GET /v1/usage</c>).</summary>
        /// <param name="from">The first day to include (inclusive). Defaults server-side to 30 days ago.</param>
        /// <param name="to">The last day to include (inclusive). Defaults server-side to today.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        Task<GetUsageResponse> GetUsageAsync(DateTime? from = null, DateTime? to = null, CancellationToken cancellationToken = default);
    }
}
