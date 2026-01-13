#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using Elympics.ElympicsSystems.Internal;
using Elympics.Models.Authentication;
using ElympicsPlayPad.ExternalCommunicators.Authentication.Models;

namespace ElympicsPlayPad.Session.Strategies
{
    internal abstract class SessionManagerInitializationStrategy
    {
        protected readonly ElympicsLoggerContext Logger;

        protected SessionManagerInitializationStrategy(ElympicsLoggerContext logger)
        {
            Logger = logger;
        }

        /// <summary>
        /// Performs post-authentication initialization tasks specific to the platform.
        /// </summary>
        /// <param name="handshake">The handshake information from authentication</param>
        /// <param name="authData">The authentication data</param>
        /// <param name="ct"></param>
        public abstract UniTask InitializePostAuthenticationAsync(
            HandshakeInfo handshake,
            AuthData authData,
            CancellationToken ct);
    }
}
