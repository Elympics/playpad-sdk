using Cysharp.Threading.Tasks;
using ElympicsPlayPad.DTO;
using ElympicsPlayPad.ExternalCommunicators.WebCommunication.Js;
using ElympicsPlayPad.Protocol;
using ElympicsPlayPad.Web3.Data;
using ElympicsPlayPad.Wrappers;

namespace ElympicsPlayPad.ExternalCommunicators.Web3.Trust
{
    internal class WebGlTrustSmartContractCommunicator : IExternalTrustSmartContractOperations
    {
        private readonly JsCommunicator _jsCommunicator;
        private readonly IElympicsLobbyWrapper _lobbyWrapper;
        public WebGlTrustSmartContractCommunicator(JsCommunicator jsCommunicator, IElympicsLobbyWrapper lobbyWrapper)
        {
            _jsCommunicator = jsCommunicator;
            _lobbyWrapper = lobbyWrapper;
        }

        public void ShowTrustPanel() => _jsCommunicator.SendVoidMessage(VoidEventTypes.IncreaseTrust,
            new IncreaseTrust
            {
                jwtToken = _lobbyWrapper.AuthData.JwtToken
            });
        public async UniTask<TrustState> GetTrustState()
        {
            var result = await _jsCommunicator.SendRequestMessage<CheckDepositMessage, CheckDepositResponse>(ReturnEventTypes.GetTrustState,
                new CheckDepositMessage()
                {
                    jwtToken = _lobbyWrapper.AuthData.JwtToken
                });

            return new TrustState()
            {
                AvailableAmount = result.Available,
                TotalAmount = result.totalAmount
            };
        }
    }
}
