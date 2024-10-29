using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using ElympicsPlayPad.DTO;
using ElympicsPlayPad.ExternalCommunicators.WebCommunication.Js;
using ElympicsPlayPad.Protocol;
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
            var message = new EncodeFunctionData()
            {
                chainId = tokenInfo.ChainId,
                contractAddress = tokenInfo.Address,
                ABI = tokenInfo.ABI,
                function = valueName,
                parameters = parameters,
            };
            return await _communicator.SendRequestMessage<EncodeFunctionData, string>(ReturnEventTypes.GetValue, message);
        }
        public async UniTask<string> GetFunctionCallData(SmartContract contract, string functionName, params object[] parameters)
        {
            _cache.Clear();
            foreach (var param in parameters)
                _cache.Add(param.ToString());

            var message = new EncodeFunctionData()
            {
                contractAddress = contract.Address,
                ABI = contract.ABI,
                function = functionName,
                parameters = _cache.ToArray(),
            };
            return await _communicator.SendRequestMessage<EncodeFunctionData, string>(ReturnEventTypes.EncodeFunctionData, message);
        }
    }
}
