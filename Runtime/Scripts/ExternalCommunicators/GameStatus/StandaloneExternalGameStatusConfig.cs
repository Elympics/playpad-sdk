using ElympicsPlayPad.ExternalCommunicators.GameStatus.Models;
using UnityEngine;

namespace ElympicsPlayPad.ExternalCommunicators.GameStatus
{
    [CreateAssetMenu(fileName = "StandaloneExternalGameStatusConfig", menuName = "Configs/Standalone/GameStatus")]
    public class StandaloneExternalGameStatusConfig : ScriptableObject
    {
        public PlayStatus PlayStatus => playStatus;
        public string LabelMessage => labelMessage;
        public bool IsHingAvailable => isHintAvailable;

        [SerializeField] private PlayStatus playStatus = DefaultPlayStatus;
        [SerializeField] private string labelMessage = DefaultLabelMessage;
        [SerializeField] private bool isHintAvailable;

        private static readonly PlayStatus DefaultPlayStatus = PlayStatus.Play;
        private static readonly string DefaultLabelMessage = "Play";
    }
}
