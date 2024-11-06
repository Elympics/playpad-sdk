#nullable enable
using ElympicsPlayPad.Web3.Data;

namespace ElympicsPlayPad.Web3.Extensions
{
    internal static class SmartContractExt
    {
        public static SmartContract ToSmartContract(this SCS.SmartContract elympicsSmartContract, string? chainId) => new()
        {
            ChainId = chainId,
            Address = elympicsSmartContract.Address,
            ABI = elympicsSmartContract.ABI
        };
    }
}
