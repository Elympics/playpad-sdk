using System;
using Cysharp.Threading.Tasks;
using Elympics.Models.Authentication;
using ElympicsPlayPad.DTO;
using ElympicsPlayPad.ExternalCommunicators.Authentication.Models;
using ElympicsPlayPad.ExternalCommunicators.Authentication.Utility;
using ElympicsPlayPad.ExternalCommunicators.WebCommunication;
using ElympicsPlayPad.ExternalCommunicators.WebCommunication.Js;
using ElympicsPlayPad.JWT;
using ElympicsPlayPad.JWT.Extensions;
using ElympicsPlayPad.Protocol;
using ElympicsPlayPad.Session.Exceptions;
using ElympicsPlayPad.Tournament.Extensions;
using ElympicsPlayPad.Utility;
using UnityEngine;

namespace ElympicsPlayPad.ExternalCommunicators.Authentication
{
    internal class WebGLExternalAuthenticator : IExternalAuthenticator
    {
        private readonly JsCommunicator _jsCommunicator;
        private IPlayPadEventListener _connectionListener = null!;
        public WebGLExternalAuthenticator(JsCommunicator jsCommunicator)
        {
            _jsCommunicator = jsCommunicator;
            _jsCommunicator.WebObjectReceived += OnWebObjectReceived;
        }

        public async UniTask<ExternalAuthData> InitializationMessage(string gameId, string gameName, string versionName, string sdkVersion, string lobbyPackageVersion)
        {
            var message = new InitializationMessage
            {
                gameId = gameId,
                gameName = gameName,
                versionName = versionName,
                sdkVersion = sdkVersion,
                lobbyPackageVersion = lobbyPackageVersion,
                systemInfo = SystemInfoDataFactory.GetSystemInfoData(),
            };
            try
            {
                var result = await _jsCommunicator.SendRequestMessage<InitializationMessage, InitializationResponse>(ReturnEventTypes.Handshake, message);
                var capabilities = (Capabilities)result.capabilities;
                var isMobile = result.device == "mobile";
                var closestRegion = result.closestRegion;
                ThrowIfInvalidResponse(result);
                var payloadDeserialized = result.authData.jwt.ExtractUnityPayloadFromJwt();
                var authType = AuthTypeRawUtility.ConvertToAuthType(payloadDeserialized.authType);
                var cached = new AuthData(Guid.Parse(result.authData.userId), result.authData.jwt, result.authData.nickname, authType);
                Debug.Log($"{nameof(WebGLExternalAuthenticator)} External authentication result: AuthType: {authType} UserId: {result.authData.userId} NickName: {result.authData.nickname}.");
                return new ExternalAuthData(cached, isMobile, capabilities, result.environment, closestRegion, result.tournamentData.ToTournamentInfo());
            }
            catch (ResponseException e)
            {
                if (e.Code == RequestErrors.ExternalAuthFailed)
                    throw new SessionManagerFatalError(e.Message);

                throw;
            }
        }
        private void ThrowIfInvalidResponse(InitializationResponse result)
        {
            if (string.IsNullOrEmpty(result.authData.jwt))
                throw new SessionManagerFatalError("External message did not return authorization token. Unable to authorize.");
            var payload = JsonWebToken.Decode(result.authData.jwt, string.Empty, false);
            if (payload is null)
                throw new SessionManagerFatalError($"Payload is null. Unable to authorize.");
            var formattedPayload = AuthTypeRawUtility.ToUnityNaming(payload);
            var payloadDeserialized = JsonUtility.FromJson<JwtPayload>(formattedPayload);
            if (string.IsNullOrEmpty(payloadDeserialized.authType))
                throw new SessionManagerFatalError("Couldn't find authType in payload. Unable to authorize.");
        }

        private void OnWebObjectReceived(WebMessageObject messageObject)
        {
            switch (messageObject.type)
            {
                case WebMessageTypes.AuthDataChanged:
                    OnAuthDataChanged(messageObject.message);
                    break;
            }
        }

        private void OnAuthDataChanged(string message)
        {
            var data = JsonUtility.FromJson<AuthDataChangedMessage>(message);
            var authType = GetAuthTypeFromJwt(data.newJwt);
            if (authType is not null)
            {
                var cached = new AuthData(Guid.Parse(data.newUserId), data.newJwt, data.newNickname, authType.Value);
                Debug.Log($"{nameof(WebGLExternalAuthenticator)} External authentication changed result: AuthType: {authType.Value} UserId: {data.newUserId} NickName: {data.newNickname}.");
                _connectionListener.OnAuthChanged(cached);
            }
        }

        private AuthType? GetAuthTypeFromJwt(string jwt)
        {
            var payload = JsonWebToken.Decode(jwt, string.Empty, false);
            var formattedPayload = AuthTypeRawUtility.ToUnityNaming(payload);
            var payloadDeserialized = JsonUtility.FromJson<JwtPayload>(formattedPayload);
            if (string.IsNullOrEmpty(payloadDeserialized.authType) is false)
            {
                var authType = AuthTypeRawUtility.ConvertToAuthType(payloadDeserialized.authType);
                return authType;
            }
            else
            {
                Debug.LogError($"{nameof(WebGLExternalAuthenticator)} Couldn't find authType in payload.");
                return null;
            }
        }

        public void Dispose()
        {
            Debug.Log($"[{nameof(WebGLExternalAuthenticator)}] Dispose.");
            _jsCommunicator.WebObjectReceived -= OnWebObjectReceived;
        }
        void IExternalAuthenticator.SetPlayPadEventListener(IPlayPadEventListener listener) => _connectionListener = listener;
    }
}
