using System;

namespace ElympicsPlayPad.Protocol.WebMessages
{
    [Serializable]
    public struct RegionUpdatedMessage
    {
        /// <summary>
        /// New Region Name
        /// </summary>
        public string region;
    }
}
