using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ElympicsPlayPad.ExternalCommunicators.Web3.NFT
{
    public abstract class CustomTonNftExternalCommunicator : MonoBehaviour, ITonNftExternalCommunicator
    {
        public abstract UniTask<bool> MintNft(string collectionAddress, string payload, CancellationToken ct = default);
    }
}
