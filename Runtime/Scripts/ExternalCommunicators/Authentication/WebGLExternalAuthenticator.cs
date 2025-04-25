#nullable enable
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Elympics.ElympicsSystems.Internal;
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
using ElympicsPlayPad.Session;
using ElympicsPlayPad.Session.Exceptions;
using UnityEngine;

namespace ElympicsPlayPad.ExternalCommunicators.Authentication
{
    internal class WebGLExternalAuthenticator : IExternalAuthenticator, IWebMessageReceiver
    {
        public event Action<string>? RegionUpdated;
        public event Action<AuthData>? AuthenticationUpdated;
        private readonly JsCommunicator _jsCommunicator;
        private readonly SessionManager _sessionManager;
        private ElympicsLoggerContext _logger;

        public WebGLExternalAuthenticator(JsCommunicator jsCommunicator, ElympicsLoggerContext logger, SessionManager sessionManager)
        {
            _jsCommunicator = jsCommunicator;
            _sessionManager = sessionManager;
            _jsCommunicator.RegisterIWebEventReceiver(this, WebMessageTypes.AuthenticationUpdated);
            _jsCommunicator.RegisterIWebEventReceiver(this, WebMessageTypes.RegionUpdated);
            _logger = logger.WithContext(nameof(WebGLExternalAuthenticator));
        }

        public async UniTask<AuthData> Authenticate(CancellationToken ct = default)
        {
            var result = await _jsCommunicator.SendRequestMessage<EmptyPayload, AuthenticationResponse>(ReturnEventTypes.GetAuthentication, null, ct);
            ThrowIfInvalidAuthenticateResponse(result);
            var payloadDeserialized = result.jwt.ExtractUnityPayloadFromJwt();
            var authType = AuthTypeRawUtility.ConvertToAuthType(payloadDeserialized.authType);
            return new AuthData(Guid.Parse(result.userId), result.jwt, result.nickname, authType);
        }
        public async UniTask ChangeRegion(string newRegion, CancellationToken ct = default)
        {
            _ = await _jsCommunicator.SendRequestMessage<ChangeRegionRequest, EmptyPayload>(ReturnEventTypes.ChangeRegion, new ChangeRegionRequest() { }, ct);
            var sessionUpdated = false;
            _sessionManager.FinishSessionInfoUpdate += OnSessionUpdated;
            RegionUpdated?.Invoke(newRegion);
            await UniTask.WaitUntil(() => sessionUpdated, PlayerLoopTiming.Update, ct);
            _sessionManager.FinishSessionInfoUpdate -= OnSessionUpdated;
            return;

            void OnSessionUpdated()
            {
                sessionUpdated = true;
            }
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
            var logger = _logger.WithMethodName();
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
                    throw logger.CaptureAndThrow(new SessionManagerFatalError(e.Message));

                throw logger.CaptureAndThrow(e);
            }
        }

        private void ThrowIfInvalidAuthenticateResponse(AuthenticationResponse result)
        {
            var logger = _logger.WithMethodName();
            if (string.IsNullOrEmpty(result.jwt))
                throw logger.CaptureAndThrow(new SessionManagerFatalError("External message did not return authorization token. Unable to authorize."));
        }

        public void OnWebMessage(WebMessage message)
        {
            var logger = _logger.WithMethodName();
            try
            {
                switch (message.type)
                {
                    case WebMessageTypes.AuthenticationUpdated:
                    {
                        var data = JsonUtility.FromJson<AuthenticationUpdatedMessage>(message.message);
                        var jwtPayload = data.jwt.ExtractUnityPayloadFromJwt();
                        var authType = AuthTypeRawUtility.ConvertToAuthType(jwtPayload.authType);
                        var cached = new AuthData(Guid.Parse(data.userId), data.jwt, data.nickname, authType);
                        AuthenticationUpdated?.Invoke(cached);
                        break;
                    }
                    case WebMessageTypes.RegionUpdated:
                    {
                        var data = JsonUtility.FromJson<RegionUpdatedMessage>(message.message);
                        RegionUpdated?.Invoke(data.region);
                        break;
                    }
                    default:
                        logger.Error($"Unable to handle message type {message.type}");
                        break;
                }

            }
            catch (Exception e)
            {
                throw logger.CaptureAndThrow(e);
            }
        }
    }
}
