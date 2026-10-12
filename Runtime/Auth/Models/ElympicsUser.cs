#nullable enable

using System;
using Elympics.Communication.Authentication.Models;

namespace ElympicsPlayPad.Auth.Models
{
    public readonly struct ElympicsUser : IEquatable<ElympicsUser>
    {
        public readonly Guid UserId;
        public readonly string Nickname;
        public readonly NicknameType NicknameType;
        public readonly string AvatarUrl;

        public ElympicsUser(Guid userId, string nickname, NicknameType nicknameType, string avatarUrl)
        {
            UserId = userId;
            Nickname = nickname;
            NicknameType = nicknameType;
            AvatarUrl = avatarUrl;
        }

        public bool Equals(ElympicsUser other) => UserId.Equals(other.UserId) && Nickname == other.Nickname && NicknameType == other.NicknameType && AvatarUrl == other.AvatarUrl;

        public override bool Equals(object? obj) => obj is ElympicsUser other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(UserId, Nickname, (int)NicknameType, AvatarUrl);

        public static bool operator ==(ElympicsUser left, ElympicsUser right) => left.Equals(right);

        public static bool operator !=(ElympicsUser left, ElympicsUser right) => !left.Equals(right);
    }
}
