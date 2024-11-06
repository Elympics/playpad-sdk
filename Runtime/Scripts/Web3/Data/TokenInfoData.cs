#nullable enable

using System.Collections.Generic;
using UnityEngine;

namespace ElympicsPlayPad.Web3.Data
{
    [CreateAssetMenu(menuName = "Scriptable Objects/Token Info Data")]
    public class TokenInfoData : ScriptableObject
    {
        public List<TokenConfig> tokenConfigs = new();

        public TokenConfig GetTokenConfig(int chainId, string address) => tokenConfigs.Find(config => config.address == address && config.chainId == chainId);
    }
}
