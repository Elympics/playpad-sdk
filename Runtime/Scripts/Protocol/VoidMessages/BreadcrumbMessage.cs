using System;

namespace ElympicsPlayPad.Protocol.VoidMessages
{
    [Serializable]
    internal struct BreadcrumbMessage
    {
        public int level;
        public string message;
    }
}
