using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using VerificahubNet.Models;
using VerificahubNet.Requests;
using VerificahubNet.Serialization;

namespace VerificahubNet
{
    /// <summary>
    /// Default <see cref="IVerificahubClient"/> implementation backed by <see cref="HttpClient"/>.
    /// Authenticates with HTTP Basic using the account's <c>api_key</c> / <c>api_secret</c>.
    /// <para>
    /// The client never mutates the <see cref="HttpClient"/> it is given — the base address and
    /// <c>Authorization</c> header are applied per request — so a single <see cref="HttpClient"/>
    /// (for example one from <c>IHttpClientFactory</c>) can be shared safely.
    /// </para>
    /// </summary>
    public sealed class VerificahubClient : IVerificahubClient, IDisposable
    {
        /// <summary>The default base URL of the Verificahub API.</summary>
        public const string DefaultBaseUrl = "https://api.verificahub.ru/";

        private readonly HttpClient _httpClient;
        private readonly bool _ownsHttpClient;
        private readonly Uri _baseAddress;
        private readonly AuthenticationHeaderValue _authorization;

        /// <summary>Creates a client that owns its own <see cref="HttpClient"/>.</summary>
        /// <param name="apiKey">The account's API key (issued in the dashboard).</param>
        /// <param name="apiSecret">The account's API secret (issued in the dashboard).</param>
        /// <param name="baseUrl">Optional override of the API base URL.</param>
        public VerificahubClient(string apiKey, string apiSecret, string baseUrl = DefaultBaseUrl)
            : this(apiKey, apiSecret, new HttpClient(), baseUrl, ownsHttpClient: true)
        {
        }

        /// <summary>
        /// Creates a client using a caller-provided <see cref="HttpClient"/> (recommended for
        /// dependency injection and <c>IHttpClientFactory</c> scenarios). The client is not disposed.
        /// </summary>
        /// <param name="apiKey">The account's API key (issued in the dashboard).</param>
        /// <param name="apiSecret">The account's API secret (issued in the dashboard).</param>
        /// <param name="httpClient">The HTTP client to use for requests; it is not mutated or disposed.</param>
        /// <param name="baseUrl">Optional override of the API base URL.</param>
        public VerificahubClient(string apiKey, string apiSecret, HttpClient httpClient, string baseUrl = DefaultBaseUrl)
            : this(apiKey, apiSecret, httpClient, baseUrl, ownsHttpClient: false)
        {
        }

        private VerificahubClient(string apiKey, string apiSecret, HttpClient httpClient, string baseUrl, bool ownsHttpClient)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new ArgumentException("API key must not be null or empty.", nameof(apiKey));
            }
            if (string.IsNullOrWhiteSpace(apiSecret))
            {
                throw new ArgumentException("API secret must not be null or empty.", nameof(apiSecret));
            }
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                throw new ArgumentException("Base URL must not be null or empty.", nameof(baseUrl));
            }

            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _ownsHttpClient = ownsHttpClient;
            _baseAddress = new Uri(baseUrl.EndsWith("/", StringComparison.Ordinal) ? baseUrl : baseUrl + "/");

            string credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes(apiKey + ":" + apiSecret));
            _authorization = new AuthenticationHeaderValue("Basic", credentials);
        }

        /// <inheritdoc />
        public Task<InitiateVerificationResponse> VerifyAsync(InitiateVerificationRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }
            return SendAsync<InitiateVerificationResponse>(HttpMethod.Post, "v1/verify", request, cancellationToken);
        }

        /// <inheritdoc />
        public Task<GetVerificationStatusResponse> GetStatusAsync(Guid requestId, CancellationToken cancellationToken = default)
            => SendAsync<GetVerificationStatusResponse>(HttpMethod.Get, "v1/verify/" + requestId.ToString("D"), null, cancellationToken);

        /// <inheritdoc />
        public Task<CheckCodeResponse> CheckCodeAsync(CheckCodeRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }
            return SendAsync<CheckCodeResponse>(HttpMethod.Post, "v1/verify/check", request, cancellationToken);
        }

        /// <inheritdoc />
        public Task<GetBalanceResponse> GetBalanceAsync(CancellationToken cancellationToken = default)
            => SendAsync<GetBalanceResponse>(HttpMethod.Get, "v1/balance", null, cancellationToken);

        /// <inheritdoc />
        public Task<GetUsageResponse> GetUsageAsync(DateTime? from = null, DateTime? to = null, CancellationToken cancellationToken = default)
        {
            var query = new List<string>(2);
            if (from.HasValue)
            {
                query.Add("from=" + from.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
            }
            if (to.HasValue)
            {
                query.Add("to=" + to.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
            }
            string path = query.Count > 0 ? "v1/usage?" + string.Join("&", query) : "v1/usage";
            return SendAsync<GetUsageResponse>(HttpMethod.Get, path, null, cancellationToken);
        }

        private async Task<TResponse> SendAsync<TResponse>(HttpMethod method, string path, object? body, CancellationToken cancellationToken)
        {
            using (var httpRequest = new HttpRequestMessage(method, new Uri(_baseAddress, path)))
            {
                httpRequest.Headers.Authorization = _authorization;

                if (body != null)
                {
                    string requestJson = JsonSerializer.Serialize(body, body.GetType(), VerificahubJson.Options);
                    httpRequest.Content = new StringContent(requestJson, Encoding.UTF8, "application/json");
                }

                HttpResponseMessage httpResponse;
                try
                {
                    httpResponse = await _httpClient.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false);
                }
                catch (HttpRequestException ex)
                {
                    throw new VerificahubTransportException($"HTTP request to '{path}' failed: {ex.Message}", ex);
                }
                catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
                {
                    throw new VerificahubTransportException($"HTTP request to '{path}' timed out: {ex.Message}", ex);
                }

                using (httpResponse)
                {
                    int statusCode = (int)httpResponse.StatusCode;
#if NET5_0_OR_GREATER
                    string responseBody = await httpResponse.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
#else
                    string responseBody = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
#endif
                    if (httpResponse.IsSuccessStatusCode)
                    {
                        TResponse? result;
                        try
                        {
                            result = JsonSerializer.Deserialize<TResponse>(responseBody, VerificahubJson.Options);
                        }
                        catch (JsonException ex)
                        {
                            throw new VerificahubProtocolException(
                                $"Failed to parse Verificahub response (HTTP {statusCode}): {ex.Message}. Body: {Truncate(responseBody)}",
                                statusCode, responseBody, ex);
                        }

                        if (result == null)
                        {
                            throw new VerificahubProtocolException(
                                $"Verificahub returned an empty response (HTTP {statusCode}).",
                                statusCode, responseBody, null);
                        }

                        return result;
                    }

                    throw CreateApiException(statusCode, responseBody);
                }
            }
        }

        private static VerificahubException CreateApiException(int statusCode, string responseBody)
        {
            ProblemDetails? problem = null;
            try
            {
                if (!string.IsNullOrWhiteSpace(responseBody))
                {
                    problem = JsonSerializer.Deserialize<ProblemDetails>(responseBody, VerificahubJson.Options);
                }
            }
            catch (JsonException)
            {
                problem = null;
            }

            if (problem == null || string.IsNullOrEmpty(problem.ErrorCode))
            {
                return new VerificahubProtocolException(
                    $"Verificahub returned HTTP {statusCode} without a parseable problem-details body. Body: {Truncate(responseBody)}",
                    statusCode, responseBody, null);
            }

            return new VerificahubApiException(
                statusCode,
                problem.ErrorCode!,
                problem.Detail,
                problem.TraceId,
                problem.AttemptsRemaining,
                responseBody,
                problem);
        }

        private const int MaxBodySnippetLength = 2048;

        private static string Truncate(string body)
        {
            if (string.IsNullOrEmpty(body))
            {
                return "<empty>";
            }
            return body.Length <= MaxBodySnippetLength
                ? body
                : body.Substring(0, MaxBodySnippetLength) + "… (truncated)";
        }

        /// <summary>Disposes the underlying <see cref="HttpClient"/> if it is owned by this client.</summary>
        public void Dispose()
        {
            if (_ownsHttpClient)
            {
                _httpClient.Dispose();
            }
        }
    }
}
