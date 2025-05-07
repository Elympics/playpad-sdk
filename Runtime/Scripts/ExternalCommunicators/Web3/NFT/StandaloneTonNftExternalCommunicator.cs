using System.Threading;
using Cysharp.Threading.Tasks;

namespace ElympicsPlayPad.ExternalCommunicators.Web3.NFT
{
    public class StandaloneTonNftExternalCommunicator : CustomTonNftExternalCommunicator
    {
        public override UniTask<bool> MintNft(string collectionAddress, string price, string payload, CancellationToken ct = default) => UniTask.FromResult(true);
        public override UniTask<string> SendRawTransaction(string address, string amount, string payload, string stateInit, CancellationToken ct = default) => UniTask.FromResult<string>(null);
    }
}
