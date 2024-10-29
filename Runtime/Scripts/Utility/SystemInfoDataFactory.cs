using ElympicsPlayPad.DTO;
using SystemInfo = UnityEngine.Device.SystemInfo;

namespace ElympicsPlayPad.Utility
{
    internal class SystemInfoDataFactory
    {
        public static SystemInfoData GetSystemInfoData() => new SystemInfoData
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
