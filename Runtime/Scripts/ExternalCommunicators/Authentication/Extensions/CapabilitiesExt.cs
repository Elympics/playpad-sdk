using ElympicsPlayPad.ExternalCommunicators.Authentication.Models;

namespace ElympicsPlayPad.ExternalCommunicators.Authentication.Extensions
{
    public static class CapabilitiesExt
    {
        public static bool IsNone(this Capabilities capabilities) => capabilities == Capabilities.None;
        public static bool IsGuest(this Capabilities capabilities) => capabilities.HasFlag(Capabilities.Guest);
        public static bool IsTelegram(this Capabilities capabilities) => capabilities.HasFlag(Capabilities.Telegram);
        public static bool IsTon(this Capabilities capabilities) => capabilities.HasFlag(Capabilities.Ton);
        public static bool IsEth(this Capabilities capabilities) => capabilities.HasFlag(Capabilities.Ethereum);

        public static bool IsOnlyGuest(this Capabilities capabilities) => capabilities == Capabilities.Guest;
        public static bool IsOnlyTelegram(this Capabilities capabilities) => capabilities == Capabilities.Telegram;
        public static bool IsOnlyTon(this Capabilities capabilities) => capabilities == Capabilities.Ton;
        public static bool IsOnlyEth(this Capabilities capabilities) => capabilities == Capabilities.Ethereum;

    }
}
