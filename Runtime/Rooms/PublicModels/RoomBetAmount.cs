using System;
using JetBrains.Annotations;

namespace ElympicsPlayPad.Rooms.Models
{
    [PublicAPI]
    public struct RoomBetAmount
    {
        public decimal BetValue;
        public Guid CoinId;
    }
}
