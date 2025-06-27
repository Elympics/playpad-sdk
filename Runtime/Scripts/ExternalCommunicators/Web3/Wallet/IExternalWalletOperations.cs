using System.Threading;
using Cysharp.Threading.Tasks;

namespace ElympicsPlayPad.ExternalCommunicators.Web3.Wallet
{
    public interface IExternalWalletOperations
    {
        UniTask<string> SignMessage(string address, string message, CancellationToken ct = default);
        UniTask<string> SendTransaction(string to, string from, string data, CancellationToken ct = default);
    }
}
