using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ElympicsPlayPad.JWT
{
    [Serializable]
    public struct ElympicsJwt
    {
        [JsonProperty("nameid")]
        public string NameId;

        [JsonProperty("auth-type")]
        public string authType;

        [JsonProperty("game-id")]
        public string gameId;

        [JsonProperty("game-name")]
        public string gameName;

        [JsonProperty("version-name")]
        public string versionName;

        [JsonProperty("chain-id")]
        public int chainId;

        [JsonProperty("eth-address")]
        public string EthAddress;

        [JsonProperty("exp")]
        [JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime expiry;
    }
}
