using ElympicsPlayPad.ExternalCommunicators.Authentication.Models;
using UnityEngine;

namespace ElympicsPlayPad.ExternalCommunicators.Authentication
{
    [CreateAssetMenu(fileName = "StandaloneExternalAuthenticatorConfig", menuName = "Configs/Standalone/Authenticator")]
    public class StandaloneExternalAuthenticatorConfig : ScriptableObject
    {
        public Capabilities Capabilities => capabilities;
        public string ClosestRegion => closestRegion;
        public FeatureAccess FeatureAccess => features;

        [SerializeField] private Capabilities capabilities;
        [SerializeField] private FeatureAccess features = DefaultFeatures;
        [SerializeField] private string closestRegion;

        private const FeatureAccess DefaultFeatures = ~FeatureAccess.None;
    }
}
