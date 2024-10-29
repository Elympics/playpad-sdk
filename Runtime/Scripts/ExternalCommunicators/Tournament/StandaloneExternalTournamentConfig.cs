using UnityEngine;

namespace ElympicsPlayPad.ExternalCommunicators.Tournament
{
    [CreateAssetMenu(fileName = "StandaloneExternalTournamentConfig", menuName = "Configs/Standalone/Tournament")]
    public class StandaloneExternalTournamentConfig : ScriptableObject
    {
        public string Id => id;
        public string TournamentName => tournamentName;
        public string OwnerId => ownerId;
        public bool IsDefault => isDefault;
        public string StartDate => startDate;
        public string EndDate => endDate;

        public int LeaderboardCapacity => leaderboardCapacity;


        [Header("Tournament")]
        [SerializeField] private string id;
        [SerializeField] private string ownerId;
        [SerializeField] private string tournamentName;
        [SerializeField] private int leaderboardCapacity;
        [SerializeField] private string startDate;
        [SerializeField] private string endDate;
        [SerializeField] private bool isDefault;
    }
}
