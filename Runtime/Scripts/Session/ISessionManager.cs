#nullable enable
using System;
using Elympics.Cysharp.Threading.Tasks;

namespace ElympicsPlayPad.Session
{
    public interface ISessionManager
    {
        event Action? StartSessionInfoUpdate;
        event Action? FinishSessionInfoUpdate;
        SessionInfo? CurrentSession { get; }
        bool ConnectedWithPlayPad { get; }
        UniTask AuthenticateFromExternalAndConnect();
    }
}
