using ElympicsPlayPad.Tournament.Data;
using UnityEngine;

namespace ElympicsPlayPad.ExternalCommunicators.Tournament
{
    [CreateAssetMenu(fileName = "StandaloneExternalTournamentConfig", menuName = "Configs/Standalone/Tournament")]
    public class StandaloneExternalTournamentConfig : ScriptableObject
    {
        private static readonly string DefaultTournamentName = "Editor tournament";
        private static readonly string DefaultStartDate = "2024-01-01T00:00:00 +00:00";
        private static readonly string DefaultEndDate = "2024-01-02T00:00:00 +00:00";
        private static readonly string DefaultPrizeDisplayName = "respect";
        private static readonly float DefaultPrizeAmount = 1750.0f;

        [Header("Tournament")]
        [SerializeField] private string id;
        [SerializeField] private string ownerId;
        [SerializeField] private string tournamentName = DefaultTournamentName;
        [SerializeField] private int leaderboardCapacity;
        [SerializeField] private string startDate = DefaultStartDate;
        [SerializeField] private string endDate = DefaultEndDate;
        [SerializeField] private bool isDefault;

        [Header("PrizePool")]
        [SerializeField] private string type;
        [SerializeField] private string displayName = DefaultPrizeDisplayName;
        [SerializeField] private Texture2D image;
        [SerializeField] private float amount = DefaultPrizeAmount;
        [SerializeField] private string description;

        public string Id => id;
        public string TournamentName => tournamentName;
        public string OwnerId => ownerId;
        public bool IsDefault => isDefault;
        public string StartDate => startDate;
        public string EndDate => endDate;
        public int LeaderboardCapacity => leaderboardCapacity;

        public PrizePoolInfo PrizePool => new()
        {
            Type = type,
            DisplayName = displayName,
            Image = image,
            Amount = amount,
            Description = description,
        };
    }
}
