#nullable enable
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Elympics.Models.Authentication;
using ElympicsPlayPad.ExternalCommunicators.Authentication.Models;
using ElympicsPlayPad.ExternalCommunicators.Authentication.Utility;
using ElympicsPlayPad.ExternalCommunicators.WebCommunication;
using ElympicsPlayPad.ExternalCommunicators.WebCommunication.Js;
using ElympicsPlayPad.JWT.Extensions;
using ElympicsPlayPad.Protocol;
using ElympicsPlayPad.Protocol.Requests;
using ElympicsPlayPad.Protocol.Responses;
using ElympicsPlayPad.Protocol.WebMessages;
using ElympicsPlayPad.Session.Exceptions;
using UnityEngine;

namespace ElympicsPlayPad.ExternalCommunicators.Authentication
{
    internal class WebGLExternalAuthenticator : IExternalAuthenticator, IWebMessageReceiver
    {
        public event Action<AuthData>? AuthenticationUpdated;
        private readonly JsCommunicator _jsCommunicator;

        public WebGLExternalAuthenticator(JsCommunicator jsCommunicator)
        {
            _jsCommunicator = jsCommunicator;
            _jsCommunicator.RegisterIWebEventReceiver(this, WebMessageTypes.AuthenticationUpdated);
        }

        public async UniTask<AuthData> Authenticate(CancellationToken ct = default)
        {
            var result = await _jsCommunicator.SendRequestMessage<EmptyPayload, AuthenticationResponse>(ReturnEventTypes.GetAuthentication, null, ct);
            ThrowIfInvalidAuthenticateResponse(result);
            var payloadDeserialized = result.jwt.ExtractUnityPayloadFromJwt();
            var authType = AuthTypeRawUtility.ConvertToAuthType(payloadDeserialized.authType);
            return new AuthData(Guid.Parse(result.userId), result.jwt, result.nickname, authType);
        }
        public async UniTask<HandshakeInfo> InitializationMessage(
            string gameId,
            string gameName,
            string versionName,
            string sdkVersion,
            string lobbyPackageVersion,
            CancellationToken ct = default)
        {
            var message = new HandshakeRequest
            {
                gameId = gameId,
                gameName = gameName,
                versionName = versionName,
                sdkVersion = sdkVersion,
                lobbyPackageVersion = lobbyPackageVersion,
            };
            try
            {
                var result = await _jsCommunicator.SendRequestMessage<HandshakeRequest, HandshakeResponse>(ReturnEventTypes.Handshake, message, ct);
                var capabilities = (Capabilities)result.capabilities;
                var isMobile = result.device == "mobile";
                var closestRegion = result.closestRegion;
                var featureAccess = (FeatureAccess)result.featureAccess;
                return new HandshakeInfo(isMobile, capabilities, result.environment, closestRegion, featureAccess);
            }
            catch (ResponseException e)
            {
                if (e.Code == RequestErrors.ExternalAuthFailed)
                    throw new SessionManagerFatalError(e.Message);

                throw;
            }
        }

        private static void ThrowIfInvalidAuthenticateResponse(AuthenticationResponse result)
        {
            if (string.IsNullOrEmpty(result.jwt))
                throw new SessionManagerFatalError("External message did not return authorization token. Unable to authorize.");
        }

        public void OnWebMessage(WebMessage message)
        {
            if (message.type != WebMessageTypes.AuthenticationUpdated)
                return;

            var data = JsonUtility.FromJson<AuthenticationUpdatedMessage>(message.message);
            var jwtPayload = data.jwt.ExtractUnityPayloadFromJwt();
            var authType = AuthTypeRawUtility.ConvertToAuthType(jwtPayload.authType);
            var cached = new AuthData(Guid.Parse(data.userId), data.jwt, data.nickname, authType);
            AuthenticationUpdated?.Invoke(cached);
        }
    }
}
