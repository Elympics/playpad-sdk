#nullable enable
using SCS;

namespace ElympicsPlayPad.Wrappers
{
    public interface ISmartContractServiceWrapper
    {
        ChainConfig? CurrentChain { get; }
        void RegisterWallet(IWallet wallet);
    }
}
