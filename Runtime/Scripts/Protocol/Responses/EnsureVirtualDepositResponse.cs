using System;
using UnityEngine.Serialization;

namespace ElympicsPlayPad.Protocol.Responses
{
    [Serializable]
    public struct EnsureVirtualDepositResponse
    {
        public bool success;
        [FormerlySerializedAs("reason")] public string error;
    }
}
