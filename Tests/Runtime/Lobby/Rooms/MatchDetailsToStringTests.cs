#nullable enable

using System;
using ElympicsPlayPad.Lobby.InternalModels.Rooms;
using ElympicsPlayPad.Rooms.Models;
using NUnit.Framework;

namespace ElympicsPlayPad.Tests.Lobby.Rooms
{
    [TestFixture]
    internal class MatchDetailsToStringTests
    {
        private const string UserSecret = "super-secret-user-secret";

        // RoomsManager logs every room update at Info, so this string reaches always-on logs.
        [Test]
        public void RoomStateWithRunningMatchShouldNotPrintUserSecret()
        {
            var roomState = Defaults.CreateRoomState(Guid.NewGuid(), Guid.NewGuid(), MatchmakingStateDto.Playing);
            var matchmakingData = roomState.MatchmakingData!;
            roomState = roomState with
            {
                MatchmakingData = matchmakingData with
                {
                    MatchData = matchmakingData.MatchData! with { MatchDetails = CreateMatchDetailsDtoWithSecret() },
                },
            };

            Assert.That(roomState.ToString(), Does.Not.Contain(UserSecret));
        }

        [Test]
        public void MatchDetailsDtoShouldNotPrintUserSecret() =>
            Assert.That(CreateMatchDetailsDtoWithSecret().ToString(), Does.Not.Contain(UserSecret));

        [Test]
        public void MatchDetailsShouldNotPrintUserSecret()
        {
            var details = new MatchDetails(new[] { Guid.NewGuid() }, "tcp", "web", UserSecret, Array.Empty<byte>(), Array.Empty<float>());

            Assert.That(details.ToString(), Does.Not.Contain(UserSecret));
        }

        private static MatchDetailsDto CreateMatchDetailsDtoWithSecret() => Defaults.CreateMatchDetails() with { UserSecret = UserSecret };
    }
}
