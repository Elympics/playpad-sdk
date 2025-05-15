using System;
namespace ElympicsPlayPad.Protocol.Responses
{
    [Serializable]
    internal struct TournamentFeeResponse
    {
        public TournamentFee[] rollings;
    }

    [Serializable]
    internal struct TournamentFee
    {
        public string rollingId;
        public string entryFee;
        public string error;
    }
}
