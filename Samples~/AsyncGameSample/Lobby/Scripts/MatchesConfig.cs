using UnityEngine;

namespace ElympicsPlayPad.Samples.AsyncGame
{
    [CreateAssetMenu(fileName = "MatchesConfig", menuName = "ElympicsPlayPad/Samples/AsyncGame/MatchesConfig")]
    public class MatchesConfig : ScriptableObject
    {
        private const string ResourcesFileName = "MatchesConfig";

        [Tooltip("If the game dependens on local state too much, making the game rejoinable won't be possible")]
        [SerializeField] private bool rejoiningEnabled = false;
        [SerializeField] private float secondsToRejoin = 120f;
        [SerializeField] private bool shouldZeroScoreBeInterpretedAsNotPlayedMatch = false;

        public bool RejoiningEnabled => rejoiningEnabled;
        public float RejoiningTimeoutInSeconds => secondsToRejoin;
        public bool ShouldZeroScoreBeInterpretedAsNotPlayedMatch => shouldZeroScoreBeInterpretedAsNotPlayedMatch;

        public static MatchesConfig LoadFromResources() => Resources.Load<MatchesConfig>(ResourcesFileName);
    }
}
