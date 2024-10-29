using Elympics.Models.Authentication;
using ElympicsPlayPad.DTO;

namespace ElympicsPlayPad.ExternalCommunicators
{
    internal interface IPlayPadEventListener
    {
        void OnWalletConnected(string address, string chainId);
        void OnWalletDisconnected();
        void OnAuthChanged(AuthData authData);
        void OnTrustDepositFinished(TrustDepositTransactionFinishedWebMessage transaction);
    }
}
