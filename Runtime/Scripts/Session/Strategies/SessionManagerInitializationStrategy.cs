#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using Elympics.Core.Logger;
using Elympics.Models.Authentication;
using ElympicsPlayPad.ExternalCommunicators.Authentication.Models;

namespace ElympicsPlayPad.Session.Strategies
{
    internal abstract class SessionManagerInitializationStrategy
    {
        protected readonly LoggerConfig Logger = ElympicsLogger.WithPlayPadSdkService();

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
