using Elympics.Models.Authentication;

namespace ElympicsPlayPad.Lobby
{
    public readonly struct ElympicsConnectionData
    {
        public AuthData AuthData { get; init; }
        public bool AutoReconnected { get; init; }

    }
}
