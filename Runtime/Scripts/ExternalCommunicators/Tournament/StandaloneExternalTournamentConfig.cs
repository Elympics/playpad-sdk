using ElympicsPlayPad.DTO;
using ElympicsPlayPad.Tournament.Data;
using UnityEngine;

namespace ElympicsPlayPad.ExternalCommunicators.Tournament
{
    [CreateAssetMenu(fileName = "StandaloneExternalTournamentConfig", menuName = "Configs/Standalone/Tournament")]
    public class StandaloneExternalTournamentConfig : ScriptableObject
    {
        public bool IsTournamentAvailable => isTournamentAvailable;
        public string Id => id;
        public string TournamentName => tournamentName;
        public string OwnerId => ownerId;
        public PrizeDto[] Prizes => prizes;
        public bool IsDefault => isDefault;
        public string LockedReason => lockedReason;
        public TournamentState TournamentState => tournamentState;
        public string CreatedDate => createdDate;
        public string StartDate => startDate;
        public string EndDate => endDate;

        public bool TonDetailsAvailable => tonDetailsAvailable;
        public bool EvmDetailsAvailable => evmDetailsAvailable;
        public TonDetailsDto TonDetails => tonDetails;
        public EvmDetailsDto EvmDetails => evmDetails;
        public int LeaderboardCapacity => leaderboardCapacity;
        public int CanPlayStatusCode => canPlayStatusCode;
        public string CanPlayMessage => canPlayMessage;



        [Header("Tournament")]
        [SerializeField] private bool isTournamentAvailable;
        [SerializeField] private string id;
        [SerializeField] private PrizeDto[] prizes;
        [SerializeField] private string ownerId;
        [SerializeField] private string tournamentName;
        [SerializeField] private int leaderboardCapacity;
        [SerializeField] private string createdDate;
        [SerializeField] private string startDate;
        [SerializeField] private string endDate;
        [SerializeField] private bool isDefault;
        [SerializeField] private string lockedReason;
        [SerializeField] private TournamentState tournamentState;
        [SerializeField] private bool tonDetailsAvailable;
        [SerializeField] private TonDetailsDto tonDetails;
        [SerializeField] private bool evmDetailsAvailable;
        [SerializeField] private EvmDetailsDto evmDetails;
        [Header("Can play response")]
        [SerializeField] private int canPlayStatusCode;
        [SerializeField] private string canPlayMessage;
    }
}
