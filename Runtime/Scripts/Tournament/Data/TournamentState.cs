#nullable enable
using JetBrains.Annotations;

namespace ElympicsPlayPad.Tournament.Data
{
    [PublicAPI]
    public enum TournamentState
    {
        Created = 0,
        EventSent = 1,
        Settled = 2,
    }
}
