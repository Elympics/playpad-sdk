using System;
using ElympicsPlayPad.ExternalCommunicators.GameStatus.Models;
using ElympicsPlayPad.Protocol.Responses;

namespace ElympicsPlayPad
{
    public static class GameStatusExtensions
    {
        public static PlayStatusInfo ToPlayStateInfo(this CanPlayGameResponse response) => new()
        {
            PlayStatus = ToPlayStatus(response.status),
            LabelInfo = response.labelMessage,
            IsHintAvailable = response.isHintAvailable,
        };

        public static PlayStatusInfo ToPlayStateInfo(this CanPlayUpdatedMessage response) => new()
        {
            PlayStatus = ToPlayStatus(response.status),
            LabelInfo = response.labelMessage,
            IsHintAvailable = response.isHintAvailable,
        };


        private static PlayStatus ToPlayStatus(string message)
        {
            var userActionRequired = message switch
            {
                "Play" => PlayStatus.Play,
                "UserActionRequired" => PlayStatus.UserActionRequired,
                "Blocked" => PlayStatus.Blocked,
                _ => throw new NotSupportedException($"{message} is not supported PlayStatus type")
            };
            return userActionRequired;
        }
    }
}
