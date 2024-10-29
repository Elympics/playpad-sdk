using System;
using ElympicsPlayPad.DTO;

namespace ElympicsPlayPad.Protocol.WebMessages.Models
{
    [Serializable]
    public class TournamentUpdatedMessage
    {
        public TournamentDataDto tournamentData;
    }
}
