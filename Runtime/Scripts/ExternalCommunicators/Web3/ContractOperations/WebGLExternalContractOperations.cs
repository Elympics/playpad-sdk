using System.Collections.Generic;
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
        private readonly JsCommunicator _communicator;
        private List<string> _cache;

        public WebGLExternalContractOperations(JsCommunicator communicator) => _communicator = communicator;

        public async UniTask<string> GetValue<TReturn>(SmartContract tokenInfo, string valueName, params string[] parameters)
        {
            var message = new EncodeFunctionDataRequest()
            {
                chainId = tokenInfo.ChainId,
                contractAddress = tokenInfo.Address,
                ABI = tokenInfo.ABI,
                function = valueName,
                parameters = parameters,
            };
            var result = await _communicator.SendRequestMessage<EncodeFunctionDataRequest, StringPayloadResponse>(ReturnEventTypes.GetValue, message);
            return result.message;
        }
        public async UniTask<string> GetFunctionCallData(SmartContract contract, string functionName, params object[] parameters)
        {
            _cache.Clear();
            foreach (var param in parameters)
                _cache.Add(param.ToString());

            var message = new EncodeFunctionDataRequest()
            {
                contractAddress = contract.Address,
                ABI = contract.ABI,
                function = functionName,
                parameters = _cache.ToArray(),
            };
            var result =  await _communicator.SendRequestMessage<EncodeFunctionDataRequest, StringPayloadResponse>(ReturnEventTypes.EncodeFunctionData, message);
            return result.message;
        }
    }
}
