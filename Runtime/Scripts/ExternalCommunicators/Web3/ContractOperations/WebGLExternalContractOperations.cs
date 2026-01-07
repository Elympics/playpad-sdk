using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ElympicsPlayPad.ExternalCommunicators.WebCommunication.Js;
using ElympicsPlayPad.Protocol;
using ElympicsPlayPad.Protocol.Requests;
using ElympicsPlayPad.Protocol.Responses;
using ElympicsPlayPad.Web3.Data;

namespace ElympicsPlayPad.ExternalCommunicators.Web3.ContractOperations
{
    internal class WebGLExternalContractOperations : IExternalContractOperations
    {
        private readonly PlayPadMessagingSystem _playPadMessagingSystem;
        private List<string> _cache;

        public WebGLExternalContractOperations(PlayPadMessagingSystem playPadMessagingSystem) => _playPadMessagingSystem = playPadMessagingSystem;

        public async UniTask<string> GetValue<TReturn>(SmartContract tokenInfo, string valueName, CancellationToken ct = default, params string[] parameters)
        {
            var message = new EncodeFunctionDataRequest
            {
                chainId = tokenInfo.ChainId,
                contractAddress = tokenInfo.Address,
                ABI = tokenInfo.ABI,
                function = valueName,
                parameters = parameters,
            };
            var result = await _playPadMessagingSystem.SendRequestMessage<EncodeFunctionDataRequest, StringPayloadResponse>(RequestResponseMessageTypes.GetValue, message, ct);
            return result.message;
        }
        public async UniTask<string> GetFunctionCallData(SmartContract contract, string functionName, CancellationToken ct = default, params object[] parameters)
        {
            _cache.Clear();
            foreach (var param in parameters)
                _cache.Add(param.ToString());

            var message = new EncodeFunctionDataRequest
            {
                contractAddress = contract.Address,
                ABI = contract.ABI,
                function = functionName,
                parameters = _cache.ToArray(),
            };
            var result = await _playPadMessagingSystem.SendRequestMessage<EncodeFunctionDataRequest, StringPayloadResponse>(RequestResponseMessageTypes.EncodeFunctionData, message, ct);
            return result.message;
        }
    }
}
