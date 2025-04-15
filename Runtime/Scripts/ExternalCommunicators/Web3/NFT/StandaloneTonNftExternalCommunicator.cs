using System.Threading;
using Cysharp.Threading.Tasks;

namespace ElympicsPlayPad.ExternalCommunicators.Web3.NFT
{
    public class StandaloneTonNftExternalCommunicator : ITonNftExternalCommunicator
    {
        public UniTask<bool> MintNft(string collectionAddress, string payload, CancellationToken ct = default) => UniTask.FromResult(true);
    }
}
