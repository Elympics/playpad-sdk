#nullable enable

using System.Numerics;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace ElympicsPlayPad.ExternalCommunicators.Web3.NFT
{
    public interface IEvmExternalCommunicator
    {
        /// <summary>Request to mint an NFT on an EVM chain.</summary>
        /// <param name="collectionAddress">NFT contract address. E.g. '0xd2135CfB216b74109775236E36d4b433F1DF507B'.</param>
        /// <param name="price">ETH in Wei 10**8 E.g. '1000000000000000000'. To convert between WEI and <see cref="decimal"/>, use <see cref="Elympics.Util.RawCoinConverter"/>.</param>
        /// <param name="chainId">ID of the chain where this NFT is deployed E.g. 2741 for Abstract.</param>
        /// <param name="data">A contract hashed method call with encoded args. E.g. '0xc02aaa39b223fe8d0a0e5c4f27ead9083c756cc2'.</param>
        /// <returns>True if minting was successful.</returns>
        UniTask<bool> MintNft(string collectionAddress, string price, BigInteger chainId, string data, CancellationToken ct = default);
    }
}
