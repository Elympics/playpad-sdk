using System;
using Elympics.ElympicsSystems.Internal;

namespace ElympicsPlayPad
{
    [Serializable]
    public struct BreadcrumbMessage
    {
        public int level;
        public string message;
        public MetaData data;
    }

    [Serializable]
    public struct MetaData
    {
        public string sessionId;
        public string app;
        public string version;
        public string gameId;
        public string userId;
        public string authType;
        public string nickName;
        public string walletAddress;
        public string time;
        public string ip; //Value assigned by playpad
        public string fleetId; //Value assigned by playpad
        public string region;
        public string lobbyUrl;
        public string roomId;
        public string queueName;
        public string matchId;
        public string tcpUdpServerAddress;
        public string webServerAddress;
        public string capabilities;
        public string tournamentId;
        public string featureAccess;
        public string context;
        public string methodName;

        internal static MetaData FromElympicsLoggerContext(ElympicsLoggerContext loggerContext) => new()
        {
            sessionId = loggerContext.SessionId.ToString(),
            app = loggerContext.App,
            gameId = loggerContext.AppContext.GameId,
            version = loggerContext.AppContext.Version,
            userId = loggerContext.UserContext.UserId,
            nickName = loggerContext.UserContext.Nickname,
            authType = loggerContext.UserContext.AuthType,
            walletAddress = loggerContext.UserContext.WalletAddress,
            region = loggerContext.ConnectionContext.Region,
            lobbyUrl = loggerContext.ConnectionContext.LobbyUrl,
            roomId = loggerContext.RoomContext.RoomId,
            queueName = loggerContext.RoomContext.QueueName,
            matchId = loggerContext.RoomContext.MatchId,
            tcpUdpServerAddress = loggerContext.RoomContext.TcpUdpServerAddress,
            webServerAddress = loggerContext.RoomContext.WebServerAddress,
            capabilities = loggerContext.PlayPadContext.Capabilities,
            tournamentId = loggerContext.PlayPadContext.TournamentId,
            featureAccess = loggerContext.PlayPadContext.FeatureAccess,
            context = loggerContext.Context,
            methodName = loggerContext.MethodName,
            time = loggerContext.Time,
        };
    }
}
