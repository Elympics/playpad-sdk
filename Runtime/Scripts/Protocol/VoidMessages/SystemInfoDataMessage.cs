using System;

namespace ElympicsPlayPad.Protocol.VoidMessages
{
    [Serializable]
    public struct SystemInfoDataMessage
    {
        public string userId;
        public string matchId;
        public SystemInfoData systemInfoData;

    }

    [Serializable]
    public struct SystemInfoData
    {
        public int systemMemorySize;
        public string operatingSystemFamily;
        public string operatingSystem;
        public int graphicsMemorySize;
        public string graphicsDeviceVersion;
        public int graphicsDeviceVendorID;
        public string graphicsDeviceVendor;
        public string graphicsDeviceName;
        public int graphicsDeviceID;
    }
}
