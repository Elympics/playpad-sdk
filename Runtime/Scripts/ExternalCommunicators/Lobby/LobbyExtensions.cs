using System;
using System.Linq;
using Elympics.Models.Matchmaking;
using ElympicsPlayPad.ExternalCommunicators.Lobby.Models;
using ElympicsPlayPad.Protocol.Responses;
namespace ElympicsPlayPad.ExternalCommunicators.Lobby
{
    internal static class LobbyExtensions
    {
        public static LobbyInfo ToLobbyInfo(this LobbyStatusResponse lobbyResponse)
        {
            var isMatchReady = !string.IsNullOrEmpty(lobbyResponse.tcpUdpServerAddress) && !string.IsNullOrEmpty(lobbyResponse.webServerAddress);

            return new LobbyInfo
            {
                IsMatchReady = isMatchReady,
                MatchData = isMatchReady ? MapToMatchmakingFinishedData(lobbyResponse) : null
            };
        }

        private static MatchmakingFinishedData MapToMatchmakingFinishedData(LobbyStatusResponse response)
        {
            var matchId = new Guid(response.matchId);
            var matchedPlayers = response.matchedPlayers?.Select(x => new Guid(x)).ToArray() ?? Array.Empty<Guid>();
            var gameEngineData = response.gameEngineData ?? Array.Empty<byte>();
            var matchmakerData = response.matchmakerData ?? Array.Empty<float>();

            return new MatchmakingFinishedData(
                matchId,
                response.userSecret,
                response.queueName,
                response.regionName,
                gameEngineData,
                matchmakerData,
                response.tcpUdpServerAddress,
                response.webServerAddress,
                matchedPlayers
            );
        }

    }
}
