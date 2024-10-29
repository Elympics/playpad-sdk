using Elympics.Models.Authentication;
using ElympicsPlayPad.ExternalCommunicators.Authentication.Models;
using UnityEngine;

namespace ElympicsPlayPad.ExternalCommunicators.Authentication
{
    [CreateAssetMenu(fileName = "StandaloneExternalAuthenticatorConfig", menuName = "Configs/Standalone/Authenticator")]
    public class StandaloneExternalAuthenticatorConfig : ScriptableObject
    {
        public AuthType AuthType => authType;
        public Capabilities Capabilities => capabilities;
        public string ClosestRegion => closestRegion;


        [SerializeField] private AuthType authType;
        [SerializeField] private Capabilities capabilities;
        [SerializeField] private string closestRegion;
    }
}
