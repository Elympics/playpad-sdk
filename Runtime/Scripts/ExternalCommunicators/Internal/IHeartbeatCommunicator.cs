using System;
namespace ElympicsPlayPad.ExternalCommunicators.Internal
{
    public interface IHeartbeatCommunicator : IDisposable
    {
        void RunHeartbeat(ushort heartbeatIntervalMs);
    }
}
