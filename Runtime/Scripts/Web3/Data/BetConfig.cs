#nullable enable

using System.Numerics;
using UnityEngine;

namespace ElympicsPlayPad.Web3.Data
{
    [CreateAssetMenu(menuName = "Scriptable Objects/Bet Config")]
    public class BetConfig : ScriptableObject
    {
        [Range(1, 128)]
        public int NumberOfPlayers;

        [SerializeField]
        [Range(0, 400)]
        private int ElympicsPerMile;

        [SerializeField]
        [Range(0, 400)]
        private int DeveloperPerMile;

        private static readonly BigInteger MaxPerMile = new(1000);
        public BigInteger TotalCommissionPerMile => new(ElympicsPerMile + DeveloperPerMile);
        public BigInteger TotalWinningPerMile => MaxPerMile - TotalCommissionPerMile;
        public BigInteger TotalCommission(BigInteger entryFee) => TotalPool(entryFee) * TotalCommissionPerMile / MaxPerMile;
        public BigInteger TotalWinPrize(BigInteger entryFee) => TotalPool(entryFee) * TotalWinningPerMile / MaxPerMile;
        public BigInteger TotalPool(BigInteger entryFee) => entryFee * new BigInteger(NumberOfPlayers);
    }
}
