using System;

namespace ElympicsPlayPad.Protocol.Requests
{
    [Serializable]
    internal struct MintNftRequest<TPayload> where TPayload : IMintNftPayload
    {
        public string collectionAddress;
        public string type;
        public TPayload payload;
    }

    internal interface IMintNftPayload { }

    [Serializable]
    internal struct MintTonNftPayload : IMintNftPayload
    {
        public string payload;
    }
}
