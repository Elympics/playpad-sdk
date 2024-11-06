using System;

namespace ElympicsPlayPad.ExternalCommunicators.Authentication.Models
{
    [Flags]
    public enum Capabilities
    {
        None = 0,
        Guest = 1,
        Telegram = 2,
        Ethereum = 4,
        Ton = 8,
    }
}
