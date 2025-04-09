#nullable enable
using ElympicsPlayPad.Protocol.Requests;
using ElympicsPlayPad.Protocol.VoidMessages;
using UnityEngine;

namespace ElympicsPlayPad.ExternalCommunicators.WebCommunication.Js
{
    public class JsCommunicationFactory
    {

        public string GetVoidMessageJson<T>(string voidMessageType, T? payload)
            where T : struct
        {
            var voidMessage = new VoidMessage<T>()
            {
                type = voidMessageType,
                payload = payload ?? default,
            };
            return JsonUtility.ToJson(voidMessage);
        }

        public string GenerateRequestMessageJson<TInput>(int requestNumber, string type, TInput? payload)
            where TInput : struct
        {
            var toSerialize = new RequestMessage<TInput>()
            {
                ticket = requestNumber,
                type = type,
                payload = payload ?? default
            };
            return JsonUtility.ToJson(toSerialize);
        }
    }
}
