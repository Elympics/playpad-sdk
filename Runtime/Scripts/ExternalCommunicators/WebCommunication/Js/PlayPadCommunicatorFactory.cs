using UnityEngine;
namespace ElympicsPlayPad.ExternalCommunicators.WebCommunication.Js
{
    internal class PlayPadCommunicatorFactory : MonoBehaviour
    {
        public IPlayPadCommunicator GetPlayPadCommunicator()
        {
            IPlayPadCommunicator playPadCommunicator = null;
#if UNITY_EDITOR
            playPadCommunicator = new ExtensionPlayPadCommunicator();
#else
                var jsCommunicator = GetComponent<JsCommunicator>();
                if (!jsCommunicator)
                    throw new System.NullReferenceException($"Cannot find component of type {nameof(JsCommunicator)} on GameObject {gameObject.name}");
                playPadCommunicator = jsCommunicator;
#endif
            return playPadCommunicator;
        }
    }
}
