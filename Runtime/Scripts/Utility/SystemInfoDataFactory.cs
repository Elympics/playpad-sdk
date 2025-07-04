using ElympicsPlayPad.Protocol.VoidMessages;
using UnityEngine;

namespace ElympicsPlayPad.Utility
{
    internal class SystemInfoDataFactory
    {
        public static SystemInfoData GetSystemInfoData() => new()
        {
            systemMemorySize = SystemInfo.systemMemorySize,
            operatingSystemFamily = SystemInfo.operatingSystemFamily.ToString(),
            operatingSystem = SystemInfo.operatingSystem,
            graphicsMemorySize = SystemInfo.graphicsMemorySize,
            graphicsDeviceVersion = SystemInfo.graphicsDeviceVersion,
            graphicsDeviceVendorID = SystemInfo.graphicsDeviceVendorID,
            graphicsDeviceVendor = SystemInfo.graphicsDeviceVendor,
            graphicsDeviceName = SystemInfo.graphicsDeviceName,
            graphicsDeviceID = SystemInfo.graphicsDeviceID
        };
    }
}
