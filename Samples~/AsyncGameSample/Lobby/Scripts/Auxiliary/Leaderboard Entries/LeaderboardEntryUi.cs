using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Assertions;

namespace ElympicsPlayPad.Samples.AsyncGame
{
    public class LeaderboardEntryUi : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI position;
        [SerializeField] private TextMeshProUGUI nickname;
        [SerializeField] private TextMeshProUGUI score;

        [Header("Top 3 badges")]
        [SerializeField] private Image badgeImage;
        [SerializeField] private Sprite goldBadge;
        [SerializeField] private Sprite silverBadge;
        [SerializeField] private Sprite bronzeBadge;

        private ILeaderboardEntryHighlighter _playerHighlighter;

        private void Awake()
        {
            _playerHighlighter = GetComponent<ILeaderboardEntryHighlighter>();
            Assert.IsNotNull(_playerHighlighter);
        }
    }
}
