#nullable enable

namespace ElympicsPlayPad.ExternalCommunicators.VirtualDeposit
{
    public readonly struct SignProofOfEntryResult
    {
        public readonly bool IsSuccess;
        public readonly string? Error;

        internal SignProofOfEntryResult(bool isSuccess, string? error)
        {
            IsSuccess = isSuccess;
            Error = error;
        }
    }
}
