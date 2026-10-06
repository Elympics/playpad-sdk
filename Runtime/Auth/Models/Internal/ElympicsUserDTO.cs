#nullable enable

using System;
using Elympics;
using Elympics.Communication.Authentication.Models;
using Elympics.Core.Logger;
using Elympics.Mappers;
using MessagePack;

namespace ElympicsPlayPad.Auth.Models.Internal
{
    /// <summary>The wire form of <see cref="ElympicsUser"/>, used by the backend and PlayPad.</summary>
    [Serializable, MessagePackObject]
    public struct ElympicsUserDTO : IEquatable<ElympicsUserDTO>
    {
        [Key(0)] public string userId;

        [Key(1)] public string nickname;

        [Key(2)] public string nicknameType;

        [Key(3)] public string avatarUrl;

        public ElympicsUserDTO(string userId, string nickname, string nicknameType, string avatarUrl)
        {
            this.userId = userId;
            this.nickname = nickname;
            this.nicknameType = nicknameType;
            this.avatarUrl = avatarUrl;
        }

        public ElympicsUser ToPublicModel()
        {
            if (!Guid.TryParse(userId, out var userGuid))
                throw new ElympicsException($"{nameof(ElympicsUserDTO)}.{nameof(ToPublicModel)} failed, because provided {nameof(userId)} is not a valid GUID string: "
                    + $"\"{userId}\". This exception is most likely caused by invalid data sent by PlayPad or Elympics backend.");

            var nickStatus = NicknameMapper.ConvertToNickNameType(nicknameType);

            if (nickStatus == NicknameType.Undefined)
                ElympicsLogger.LogError($"Unexpected {nameof(nicknameType)} received: {nicknameType}.");

            return new ElympicsUser(userGuid, nickname, nickStatus, avatarUrl);
        }

        public override string ToString() => $"{nameof(userId)}:{Environment.NewLine}\t{userId}{Environment.NewLine}"
            + $"{nameof(nickname)}:{nickname}{Environment.NewLine}"
            + $"{nameof(nicknameType)}:{nicknameType}{Environment.NewLine}"
            + $"{nameof(avatarUrl)}:{avatarUrl}{Environment.NewLine}";
        public bool Equals(ElympicsUserDTO other) => userId == other.userId && nickname == other.nickname && nicknameType == other.nicknameType && avatarUrl == other.avatarUrl;
        public override bool Equals(object? obj) => obj is ElympicsUserDTO other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(userId, nickname, nicknameType, avatarUrl);
    }
}
