using System.Collections.Generic;
using Elympics;
using ElympicsPlayPad.ExternalCommunicators.Authentication.Models;
using ElympicsPlayPad.Session;
using UnityEngine;
namespace ElympicsPlayPad.ExternalCommunicators
{
    public class AuthFactory
    {
        private readonly Dictionary<LaunchMode, ISessionManagerAuthProvider> _authProviders = new();

        public void RegisterAuthProvider(ISessionManagerAuthProvider provider, LaunchMode mode)
        {
            if (_authProviders.TryAdd(mode, provider) == false)
                Debug.LogError($"Auth provider already existing for LaunchMode {mode}");
        }

        public ISessionManagerAuthProvider GetAuthProvider(LaunchMode mode)
        {
            if (_authProviders.TryGetValue(mode, out var provider))
                return provider;
            throw new ElympicsException("No auth provider for mode {mode}");
        }
    }
}
