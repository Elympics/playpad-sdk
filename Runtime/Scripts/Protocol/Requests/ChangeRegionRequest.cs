using System;

namespace ElympicsPlayPad.Protocol.Requests
{
    [Serializable]
    public struct ChangeRegionRequest
    {
        public string newRegion;
    }
}
