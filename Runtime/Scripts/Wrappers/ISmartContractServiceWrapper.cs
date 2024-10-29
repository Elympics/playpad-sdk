using Cysharp.Threading.Tasks;
using ElympicsPlayPad.DTO;
using ElympicsPlayPad.Web3.Data;
using SCS;

namespace ElympicsPlayPad.Wrappers
{
    public interface ISmartContractServiceWrapper
    {
        public ChainConfig? CurrentChain { get; }
        public void RegisterWallet(IWallet wallet);
        UniTask<TrustState> GetTrustBalance();
    }
}
