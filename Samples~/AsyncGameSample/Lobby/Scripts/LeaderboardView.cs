using System.Collections.Generic;
using TMPro;
using UnityEngine;

#nullable enable

namespace ElympicsPlayPad.Samples.AsyncGame
{
    public class LeaderboardView : MonoBehaviour
    {
        // Should reference exactly 6 entries
        [SerializeField] private List<LeaderboardEntryUi>? displayedEntries;

        [SerializeField] private TextMeshProUGUI? numberOfParticipants;

        [SerializeField] private GameObject? separator;
    }
}
