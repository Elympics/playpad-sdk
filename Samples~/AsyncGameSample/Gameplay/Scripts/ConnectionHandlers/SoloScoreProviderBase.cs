using Elympics;

namespace ElympicsPlayPad.Samples.AsyncGame
{
    public abstract class SoloScoreProviderBase : ElympicsMonoBehaviour
    {
        public abstract float Score { get; }
    }
}
