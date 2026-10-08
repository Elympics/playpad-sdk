#nullable enable
using System;

namespace ElympicsPlayPad.ExternalCommunicators.Replay
{
    public interface IExternalReplayCommunicator
    {
        event Action? ReplayRetrieved;
        void PlayReplay();
    }
}
