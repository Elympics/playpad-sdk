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

        /// <summary>
        /// <c>true</c> in an offline game mode, set by the host or the <c>ELYMPICS_GAME_MODE</c> override: <see cref="AuthenticateFromExternalAndConnect"/>
        /// stopped after the handshake, so there is no authentication and <see cref="CurrentSession"/> stays <c>null</c> (see <c>offline-game-modes.md</c>).
        /// </summary>
        bool IsOffline => false;

        /// <summary>
        /// The offline game mode (for example <c>onboarding</c>), or <c>null</c> in the standard online flow.
        /// </summary>
        string? GameMode => null;

        UniTask AuthenticateFromExternalAndConnect();
    }
}
