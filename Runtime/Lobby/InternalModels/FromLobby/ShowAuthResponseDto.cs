#nullable enable

using System;
using ElympicsPlayPad.Auth.Models.Internal;
using MessagePack;

namespace ElympicsPlayPad.Lobby.InternalModels.FromLobby
{
    [MessagePackObject]
    public class ShowAuthResponseDto : ILobbyResponse
    {
        [Key(1)] public string AuthType { get; set; }
        [Key(2)] public string? EthAddress { get; set; }
        [Key(5)] public Guid RequestId { get; set; }
        [Key(6)] public ElympicsUserDTO User { get; set; }

        public ShowAuthResponseDto() { }
        public ShowAuthResponseDto(string authType, string? ethAddress, Guid requestId, ElympicsUserDTO user)
        {
            AuthType = authType;
            EthAddress = ethAddress;
            RequestId = requestId;
            User = user;
        }
    }
}
