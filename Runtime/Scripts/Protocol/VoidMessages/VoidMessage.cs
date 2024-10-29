using System;
using UnityEngine.Serialization;

namespace ElympicsPlayPad.Protocol.VoidMessages
{
    [Serializable]
    public class VoidMessage<T>
    {
        public string type;
        public T payload;
    }
}
