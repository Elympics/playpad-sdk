#if UNITY_EDITOR || !UNITY_WEBGL || ELYMPICS_PLAYPAD_KEEP_LOBBY
#nullable enable

using System;
using Cysharp.Threading.Tasks;
using Elympics;
using Elympics.Core.Logger;
using Elympics.Models.Authentication;
using ElympicsPlayPad.Auth;
using ElympicsPlayPad.Auth.Models;

namespace ElympicsPlayPad.Wrappers.Connection
{
    internal abstract class AuthorizationStrategy
    {
        private readonly IAuthClient _authClient;
        private readonly string _clientSecret;

        protected AuthorizationStrategy(IAuthClient authClient, string clientSecret)
        {
            _authClient = authClient;
            _clientSecret = clientSecret;
        }

        public abstract UniTask<Result<AuthData?, string>> Authorize(ConnectionData data);

        protected async UniTask<Result<AuthData?, string>> AuthenticateWithCachedData(CachedAuthData data)
        {
            var cachedData = data.CachedData;
            ElympicsLogger.LogInfo($"Starting cached {cachedData.AuthType} authentication...");

            try
            {
                if (!JwtTokenUtil.IsJwtExpired(cachedData.JwtToken))
                    return Result<AuthData?, string>.Success(cachedData);
                if (data.AutoRetryIfExpired)
                    return await AuthenticateWithAsync(cachedData.AuthType);

                return Result<AuthData?, string>.Failure("JWT Token has expired.");
            }
            catch (Exception e)
            {
                throw new ElympicsException("Authentication failed. Reason:" + Environment.NewLine + e);
            }
        }

        protected async UniTask<Result<AuthData?, string>> AuthenticateWithAsync(AuthType authType)
        {
            try
            {
                switch (authType)
                {
                    case AuthType.ClientSecret:
                        var result = await _authClient.AuthenticateWithClientSecret(_clientSecret);
                        return result.IsSuccess ? Result<AuthData?, string>.Success(result.Value) : Result<AuthData?, string>.Failure(result.Error);
                    case AuthType.None:
                        return Result<AuthData?, string>.Success(null);
                    case AuthType.EthAddress:
                    case AuthType.Telegram:
                    default:
                        return Result<AuthData?, string>.Failure($"Implicit authorization is not supported for the specified authentication type: {authType}.");
                }
            }
            catch (Exception e)
            {
                throw new ElympicsException("Authentication failed. Reason:" + Environment.NewLine + e);
            }
        }
    }

    internal class AuthorizedStrategy : AuthorizationStrategy
    {
        private readonly AuthData _currentAuthData;

        public AuthorizedStrategy(AuthData currentAuthData, IAuthClient authClient, string clientSecret) : base(authClient, clientSecret) =>
            _currentAuthData = currentAuthData;

        public override UniTask<Result<AuthData?, string>> Authorize(ConnectionData data)
        {
            ElympicsLogger.LogWarning("Already authorized. Using existing AuthData. To change your authorization data, sign out first.");
            return UniTask.FromResult(Result<AuthData?, string>.Success(_currentAuthData));
        }
    }

    internal class UnauthorizedStrategy : AuthorizationStrategy
    {
        public UnauthorizedStrategy(IAuthClient authClient, string clientSecret) : base(authClient, clientSecret)
        { }

        public override async UniTask<Result<AuthData?, string>> Authorize(ConnectionData data)
        {
            if (data.AuthFromCacheData.HasValue)
                return await AuthenticateWithCachedData(data.AuthFromCacheData.Value);
            if (data.AuthType.HasValue)
                return await AuthenticateWithAsync(data.AuthType.Value);

            return Result<AuthData?, string>.Failure("No data for authentication");
        }
    }
}
#endif
