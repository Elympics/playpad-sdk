using System;

namespace ElympicsPlayPad.Protocol.Requests
{
    [Serializable]
    public struct MintNftRequest<TPayload> where TPayload : IMintNftPayload
    {
        public string collectionAddress;
        public string price;
        public string type;
        public TPayload payload;
    }

    public interface IMintNftPayload { }

    [Serializable]
    public struct MintTonNftPayload : IMintNftPayload
    {
        public string payload;
    }
}
