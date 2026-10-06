#if UNITY_EDITOR || !UNITY_WEBGL || ELYMPICS_PLAYPAD_KEEP_LOBBY
#nullable enable

using System;
using Cysharp.Threading.Tasks;
using Elympics;
using Elympics.Core.Logger;
using ElympicsPlayPad.Lobby;
using ElympicsPlayPad.Lobby.InternalModels.FromLobby;

namespace ElympicsPlayPad.Wrappers.Connection
{
    internal abstract class ConnectionStrategy
    {
        private readonly WebSocketSession _webSocketSession;

        protected ConnectionStrategy(WebSocketSession webSocketSession) => _webSocketSession = webSocketSession;

        /// <returns>Server-side game details, or null if <paramref name="newConnectionDetails"/> did not change since the last call.</returns>
        public abstract UniTask<GameDataResponseDto?> Connect(SessionConnectionDetails newConnectionDetails);

        protected async UniTask<GameDataResponseDto?> ConnectToLobby(SessionConnectionDetails connectionDetails) =>
            await _webSocketSession.Connect(connectionDetails);

        protected void DisconnectFromLobby(DisconnectionReason reason)
        {
            if (_webSocketSession.IsConnected)
                _webSocketSession.Disconnect(reason);
        }
    }

    internal class AuthorizedConnectedSocketConnectionStrategy : ConnectionStrategy
    {
        private readonly SessionConnectionDetails _currentSession;

        public AuthorizedConnectedSocketConnectionStrategy(WebSocketSession webSocketSession, SessionConnectionDetails currentSession) : base(webSocketSession) =>
            _currentSession = currentSession;

        public override async UniTask<GameDataResponseDto?> Connect(SessionConnectionDetails newConnectionDetails)
        {
            if (!_currentSession.Equals(newConnectionDetails))
            {
                DisconnectFromLobby(DisconnectionReason.Reconnection);
                return await ConnectToLobby(newConnectionDetails);
            }
            ElympicsLogger.LogWarning("No change in connection data.");
            return null;
        }
    }

    internal class AuthorizedNotConnectedStrategy : ConnectionStrategy
    {
        public AuthorizedNotConnectedStrategy(WebSocketSession socketSession) : base(socketSession)
        { }

        public override async UniTask<GameDataResponseDto?> Connect(SessionConnectionDetails newConnectionDetails)
        {
            try
            {
                return await ConnectToLobby(newConnectionDetails);
            }
            catch (Exception e)
            {
                throw ElympicsLogger.LogExceptionAndReturn(e);
            }
        }
    }

    internal class UnauthorizedSocketConnectionStrategy : ConnectionStrategy
    {
        public UnauthorizedSocketConnectionStrategy(WebSocketSession socketSession) : base(socketSession)
        { }

        public override UniTask<GameDataResponseDto?> Connect(SessionConnectionDetails newConnectionDetails) =>
            throw new ElympicsException("Connecting canceled because user is not authenticated.");
    }
}
#endif
