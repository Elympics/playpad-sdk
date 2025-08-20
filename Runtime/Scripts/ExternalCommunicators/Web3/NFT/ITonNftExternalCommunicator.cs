#nullable enable

using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace ElympicsPlayPad.ExternalCommunicators.Web3.NFT
{
    public interface ITonNftExternalCommunicator
    {
        UniTask<bool> MintNft(string collectionAddress, string price, string payload, CancellationToken ct = default);

        [Obsolete("This API is experimental and subject to change without notice in future releases.")]
        UniTask<string> SendRawTransaction(string address, string amount, string payload, string? stateInit, CancellationToken ct = default);
    }
}
