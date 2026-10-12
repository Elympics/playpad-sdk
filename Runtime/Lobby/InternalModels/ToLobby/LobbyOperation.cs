#nullable enable

using System;
using MessagePack;

namespace ElympicsPlayPad.Lobby.InternalModels.ToLobby
{
    public abstract record LobbyOperation([property: Key(0)] Guid OperationId) : IToLobby
    {
        protected LobbyOperation() : this(Guid.NewGuid())
        { }
    }
}
