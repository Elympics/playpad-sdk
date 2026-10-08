#nullable enable

using System;
using System.Reflection;
using System.Threading;
using Castle.Core.Internal;
using Cysharp.Threading.Tasks;
using Elympics.Core.Logger;
using ElympicsPlayPad.Lobby;
using ElympicsPlayPad.Lobby.InternalModels.Rooms.FromRooms;
using ElympicsPlayPad.Rooms;
using ElympicsPlayPad.Tests.Lobby.Common.RoomMocks;
using NSubstitute;
using NSubstitute.ClearExtensions;
using NUnit.Framework;

namespace ElympicsPlayPad.Tests.Lobby.Rooms
{
    internal abstract class TestRoomsManager
    {
        protected static readonly Guid HostId = new("10100000000000000000000000aaaa01");
        protected static readonly Guid RoomId = new("10100000000000000000000000bbbb01");
        protected static DiscreteTimer Timer = new();

        protected static readonly RoomStateChangedDto InitialRoomState = Defaults.CreateRoomState(RoomId, HostId)
            .WithLastUpdate(Timer++);

        protected readonly RoomsManager RoomsManager;
        protected readonly IRoomJoiner RoomJoiner;
        protected readonly IMatchLauncher MatchLauncherMock;
        protected readonly IRoomsClient RoomsClientMock;
        protected readonly EventObserver<IRoomsManager> EventRegister;
        private CancellationTokenSource _tearDownCts = new();

        private static readonly TimeSpan StandardResponseDelay = TimeSpan.FromMilliseconds(200);
        protected UniTask GetResponseDelay() => UniTask.Delay(StandardResponseDelay, DelayType.DeltaTime, cancellationToken: _tearDownCts.Token);
        protected UniTask GetEternalDelay() => UniTask.Never(_tearDownCts.Token);

        private readonly FieldInfo _roomsManagerInitialized;
        private void InitializeRoomsManager() => _roomsManagerInitialized.SetValue(RoomsManager, true);

        protected TestRoomsManager()
        {
            ElympicsLogger.State.SetTcpUdp();
            MatchLauncherMock = Substitute.For<IMatchLauncher>();
            RoomsClientMock = Substitute.For<IRoomsClient>();
            RoomJoiner = new RoomJoiner(RoomsClientMock)
            {
                OperationTimeout = TimeSpan.FromSeconds(1),
            };
            RoomsManager = new RoomsManager(MatchLauncherMock, RoomsClientMock, RoomJoiner);
            LobbyTimeout.RoomStateChangeConfirmationTimeout = TimeSpan.FromSeconds(1);
            EventRegister = new EventObserver<IRoomsManager>(RoomsManager);

            _roomsManagerInitialized = RoomsManager.GetType()
                .GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                .Find(x => x.Name == "_initialized");
        }

        [SetUp]
        protected void SetUp()
        {
            _tearDownCts = new CancellationTokenSource();
            InitializeRoomsManager();
        }

        [TearDown]
        protected void Reset()
        {
            _tearDownCts.Cancel();
            ((IRoomsManager)RoomsManager).Reset();
            EventRegister.Reset();
            RoomsClientMock.ClearSubstitute();
            MatchLauncherMock.ClearSubstitute();
        }

        /// <param name="updateLastUpdateTime">Stamps the state with the current <see cref="Timer"/> value.</param>
        protected void EmitRoomUpdate(RoomStateChangedDto updatedState, bool updateLastUpdateTime = true) =>
            RoomsClientMock.RoomStateChanged += Raise.Event<Action<RoomStateChangedDto>>(updateLastUpdateTime ? updatedState.WithLastUpdate(Timer++) : updatedState);

        /// <summary>Moves the joined room from JoinedNoTracking to JoinedWithTracking by emitting its first state update.</summary>
        protected void SetRoomAsTrackedWhenItGetsJoined(RoomStateChangedDto? roomState = null)
        {
            roomState ??= InitialRoomState;
            RoomJoiner.JoiningStateChanged += OnJoiningStateChanged;
            return;

            void OnJoiningStateChanged(RoomJoiningState state)
            {
                if (state is not RoomJoiningState.JoinedNoTracking)
                    return;
                EmitRoomUpdate(roomState);
                RoomJoiner.JoiningStateChanged -= OnJoiningStateChanged;
            }
        }
    }
}
