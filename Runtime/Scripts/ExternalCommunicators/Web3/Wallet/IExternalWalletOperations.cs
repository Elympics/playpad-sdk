
using Cysharp.Threading.Tasks;

namespace ElympicsPlayPad.ExternalCommunicators.Web3.Wallet
{
    public interface IExternalWalletOperations
    {
        public UniTask<string> SignMessage<TInput>(string address, TInput message);
        public UniTask<string> SendTransaction(string to, string from, string data);
    }
}
