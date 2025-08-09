using System.Numerics;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace ElympicsPlayPad.ExternalCommunicators.Web3.NFT
{
    public class StandaloneEvmExternalCommunicator : IEvmExternalCommunicator
    {
        public UniTask<bool> MintNft(string collectionAddress, string price, BigInteger chainId, string data, CancellationToken ct = default) => UniTask.FromResult(true);
    }
}
