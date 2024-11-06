using ElympicsPlayPad.ExternalCommunicators.GameStatus.Models;
using UnityEngine;
using UnityEngine.Serialization;

namespace ElympicsPlayPad.ExternalCommunicators.GameStatus
{
    [CreateAssetMenu(fileName = "StandaloneExternalGameStatusConfig", menuName = "Configs/Standalone/GameStatus")]
    public class StandaloneExternalGameStatusConfig : ScriptableObject
    {
        public PlayStatus PlayStatus => playStatus;
        public string LabelMessage => labelMessage;

        [FormerlySerializedAs("playState")] [SerializeField] private PlayStatus playStatus;
        [SerializeField] private string labelMessage;
    }
}
