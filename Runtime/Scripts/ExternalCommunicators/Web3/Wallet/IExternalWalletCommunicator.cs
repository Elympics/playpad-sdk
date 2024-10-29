#nullable enable
using System;
using Cysharp.Threading.Tasks;
using ElympicsPlayPad.DTO;

namespace ElympicsPlayPad.ExternalCommunicators.Web3.Wallet
{
    public interface IExternalWalletCommunicator : IExternalWalletOperations, IDisposable
    {
        public UniTask<ConnectionResponse> Connect();
        public void ExternalShowChainSelection();
        public void ExternalShowConnectToWallet();
        public void ExternalShowAccountInfo();
        internal void SetPlayPadEventListener(IPlayPadEventListener listener)
        {

        }
    }
}
