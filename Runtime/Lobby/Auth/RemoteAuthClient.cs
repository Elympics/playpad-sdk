using System;
using System.Threading;
using Elympics;
using Elympics.Cysharp.Threading.Tasks;
using Elympics.Models.Authentication;
using ElympicsPlayPad.Auth.Models;
using AuthRoutes = ElympicsPlayPad.Auth.Models.Routes;

#nullable enable

namespace ElympicsPlayPad.Auth
{
    internal class RemoteAuthClient : IAuthClient
    {
        internal static TimeSpan AuthenticationTimeout = TimeSpan.FromSeconds(30);

        private readonly string _clientSecretAuthUrl;

        public RemoteAuthClient(string authEndpoint)
        {
            var uriBuilder = new UriBuilder(authEndpoint);
            var oldPath = uriBuilder.Path.TrimEnd('/');
            uriBuilder.Path = string.Join("/", oldPath, AuthRoutes.Base, AuthRoutes.ClientSecretAuth);
            _clientSecretAuthUrl = uriBuilder.Uri.ToString();
        }

        public async UniTask<Result<AuthData, string>> AuthenticateWithClientSecret(string clientSecret, CancellationToken ct = default)
        {
            using var delayCts = new CancellationTokenSource();
            using var timer = delayCts.CancelAfterSlim(AuthenticationTimeout, DelayType.Realtime);
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, delayCts.Token);

            var putTcs = new UniTaskCompletionSource<Result<AuthenticationDataResponse, Exception>>();
            var requestModel = new ClientSecretAuthRequest { clientSecret = clientSecret };
            ElympicsWebClient.SendPutRequest<AuthenticationDataResponse>(_clientSecretAuthUrl, requestModel, null, result => putTcs.TrySetResult(result), ct: linkedCts.Token);
            var putTcsResult = await putTcs.Task.AttachExternalCancellation(linkedCts.Token).SuppressCancellationThrow();
            if (!putTcsResult.IsCanceled)
                return putTcsResult.Result.IsSuccess
                    ? Result<AuthData, string>.Success(putTcsResult.Result.Value.ToAuthData(AuthType.ClientSecret))
                    : Result<AuthData, string>.Failure(putTcsResult.Result.Error.Message);

            return Result<AuthData, string>.Failure(ct.IsCancellationRequested ? "Request cancelled." : "Timeout.");
        }
    }
}
