#nullable enable
using SCS;

namespace ElympicsPlayPad.Wrappers
{
    public interface ISmartContractServiceWrapper
    {
        public ChainConfig? CurrentChain { get; }
        public void RegisterWallet(IWallet wallet);
    }
}
