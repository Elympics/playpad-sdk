#nullable enable
#pragma warning disable 67 //An event was declared but never used in the class in which it was declared.
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Elympics.Communication.Authentication.Models;
using Elympics.Communication.Authentication.Models.Internal;
using ElympicsPlayPad.Leaderboard;
using ElympicsPlayPad.Leaderboard.Extensions;
using ElympicsPlayPad.Protocol.Responses;

namespace ElympicsPlayPad.ExternalCommunicators.Leaderboard
{
    public class StandaloneLeaderboardCommunicator : CustomStandaloneLeaderboardCommunicatorBase
    {
        public override event Action<LeaderboardStatusInfo>? LeaderboardUpdated;
        public override event Action<UserHighScoreInfo>? UserHighScoreUpdated;
        public override UserHighScoreInfo? UserHighScore => _userHighScoreInfo;
        private UserHighScoreInfo? _userHighScoreInfo;
        public override LeaderboardStatusInfo? Leaderboard => _leaderboard;
        private LeaderboardStatusInfo? _leaderboard;

        public override UniTask<LeaderboardStatusInfo> FetchLeaderboard(CancellationToken ct = default)
        {
            _leaderboard = new LeaderboardResponse()
            {
                entries = new[]
                {
                    new Entry
                    {
                        position = 1,
                        score = 10,
                        scoredAt = DateTime.UtcNow.ToString("o"),
                        matchId = "00000000-0000-0000-0001-000000000000",
                        tournamentId = "abcdef",
                        user = new ElympicsUserDTO(Guid.Empty.ToString(), "TestNickName", nameof(NicknameType.Common), "testAvatarURL")
                    }
                },
                participants = 10,
            }.MapToLeaderboardStatus();
            return UniTask.FromResult(Leaderboard.Value);
        }
        public override UniTask<UserHighScoreInfo?> FetchUserHighScore(CancellationToken ct = default)
        {
            _userHighScoreInfo = new UserHighScoreInfo()
            {
                Points = 99,
                ScoredAt = DateTime.Now - TimeSpan.FromDays(1),
            };
            return UniTask.FromResult(UserHighScore);
        }
    }
}
