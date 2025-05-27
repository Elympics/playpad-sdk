using System;
namespace ElympicsPlayPad.Protocol.Requests
{
    [Serializable]
    internal struct TournamentFeeRequest
    {
        public RollingDetail[] rollings;
    }

    [Serializable]
    internal struct RollingDetail
    {
        public string coinId;
        public int playersCount;
        public string prize;

    }
}
