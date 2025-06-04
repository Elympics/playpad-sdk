#nullable enable

using UnityEngine;

namespace ElympicsPlayPad.Web3.Data.Providers
{
    public class TokenInfo : MonoBehaviour
    {
        [SerializeField] private TokenInfoData tokenInfoData = null!;
        private ITokenAddressProvider _tokenAddressProvider = null!;

        public string Symbol => TokenConfig.symbol;
        public string Name => TokenConfig.name;
        public int Decimals => TokenConfig.decimals;
        public string Address => TokenConfig.address;
        public int ChainId => TokenConfig.chainId;

        private void Awake()
        {
            _tokenAddressProvider = GetComponent<ITokenAddressProvider>();
        }

        private TokenConfig TokenConfig
        {
            get
            {
                if (_tokenConfig is not null)
                    return _tokenConfig.Value;
                var chainId = _tokenAddressProvider.GetChainId();
                var tokenAddress = _tokenAddressProvider.GetAddress();
                _tokenConfig = tokenInfoData.GetTokenConfig(chainId, tokenAddress);
                return _tokenConfig.Value;
            }
        }

        private TokenConfig? _tokenConfig;
    }
}
