using System.Numerics;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ElympicsPlayPad.ExternalCommunicators.Web3.NFT
{
    public abstract class CustomEvmExternalCommunicator : MonoBehaviour, IEvmExternalCommunicator
    {
        public abstract UniTask<bool> MintNft(string collectionAddress, string price, BigInteger chainId, string data, CancellationToken ct = default);
        public abstract UniTask<string> SendRawTransaction(string address, string amount, string chainId, string data, CancellationToken ct = default);
    }
}
