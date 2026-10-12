using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Elympics;
using Elympics.Models.Authentication;
using ElympicsPlayPad.Auth;
using NSubstitute;

namespace ElympicsPlayPad.Tests.Lobby.Common.MockSetup
{
    internal static class AuthClientMockSetup
    {
        /// <summary>A none-alg (but valid) token with exp set to year 2288.</summary>
        public const string NonExpiringToken = "eyJhbGciOiJub25lIn0.eyJleHAiOjEwMDM5OTk5OTk5fQ.";

        public static IAuthClient CreateSuccessIAuthClient(this IAuthClient mock, Guid userId)
        {
            var result = UniTask.FromResult(Result<AuthData, string>.Success(new AuthData(userId, NonExpiringToken, AuthType.ClientSecret)));
            _ = mock.AuthenticateWithClientSecret(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(result);
            return mock;
        }

        public static IAuthClient CreateFailureIAuthClient(this IAuthClient mock)
        {
            var result = UniTask.FromResult(Result<AuthData, string>.Failure("Failed to authenticate with clientSecret"));
            _ = mock.AuthenticateWithClientSecret(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(result);
            return mock;
        }
    }
}
