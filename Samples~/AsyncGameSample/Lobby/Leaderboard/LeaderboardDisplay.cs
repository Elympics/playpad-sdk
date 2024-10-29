using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Elympics;

namespace ElympicsPlayPad.Samples.AsyncGame
{
    public class LeaderboardDisplay : MonoBehaviour
    {
        [SerializeField] private List<TextPair> leaderBoardContent;

        public void DisplayTop5Entries(LeaderboardFetchResult result)
        {
            foreach (var leaderboardRow in leaderBoardContent)
            {
                leaderboardRow.Item1.text = "";
                leaderboardRow.Item2.text = "";
            }

            for (int i = 0; i < 5 && i < result.Entries.Count; i++)
            {
                leaderBoardContent[i].Item1.text = result.Entries[i].Nickname;
                leaderBoardContent[i].Item2.text = result.Entries[i].Score.ToString();
            }
        }

        [System.Serializable]
        private struct TextPair
        {
            public TextMeshProUGUI Item1;
            public TextMeshProUGUI Item2;
        }
    }
}
