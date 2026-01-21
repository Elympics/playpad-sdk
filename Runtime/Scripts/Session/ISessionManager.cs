#nullable enable
using System;
using Cysharp.Threading.Tasks;
namespace ElympicsPlayPad.Session
{
    public interface ISessionManager
    {
        public event Action? StartSessionInfoUpdate;
        public event Action? FinishSessionInfoUpdate;
        SessionInfo? CurrentSession { get; }
        public bool ConnectedWithPlayPad { get; }
        public UniTask AuthenticateFromExternalAndConnect();
    }
}
