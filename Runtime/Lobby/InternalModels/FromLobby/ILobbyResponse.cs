#nullable enable

using System;

namespace ElympicsPlayPad.Lobby.InternalModels.FromLobby
{
    internal interface ILobbyResponse : IFromLobby
    {
        Guid RequestId { get; }
    }
}
