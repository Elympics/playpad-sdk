#nullable enable

using System.Threading;
using Cysharp.Threading.Tasks;

namespace ElympicsPlayPad.ExternalCommunicators.Web3.NFT
{
    public interface ITonNftExternalCommunicator
    {
        UniTask<bool> MintNft(string collectionAddress, string price, string payload, CancellationToken ct = default);
    }
}
