using System;
namespace ElympicsPlayPad.ExternalCommunicators.Authentication.Models
{
    [Flags]
    public enum LaunchMode
    {
        None = 0,
        Gameplay = 1 << 0,
        Lobby = 1 << 1,
    }
}
