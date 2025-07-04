using System;
namespace ElympicsPlayPad.Protocol.Responses
{
    [Serializable]
    public struct GetRollingTournamentUnreadSettlementsResponse
    {
        public int unreadSettledCount;
    }
}
