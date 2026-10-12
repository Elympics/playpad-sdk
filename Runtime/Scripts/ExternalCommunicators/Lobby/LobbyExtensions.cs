using System;
using System.Linq;
using ElympicsPlayPad.ExternalCommunicators.Lobby.Models;
using ElympicsPlayPad.Protocol.Responses;
using ElympicsPlayPad.Wrappers;

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
                MatchData = isMatchReady ? MapToMatchData(lobbyResponse) : null,
            };
        }

        private static FoundMatch MapToMatchData(LobbyStatusResponse response)
        {
            var matchId = new Guid(response.matchId);
            var matchedPlayers = response.matchedPlayers?.Select(x => new Guid(x)).ToArray() ?? Array.Empty<Guid>();
            var gameEngineData = response.gameEngineData ?? Array.Empty<byte>();
            var matchmakerData = response.matchmakerData ?? Array.Empty<float>();

            return new FoundMatch(matchId, matchedPlayers, response.tcpUdpServerAddress, response.webServerAddress, response.userSecret, gameEngineData, matchmakerData,
                response.queueName, response.regionName);
        }
    }
}
