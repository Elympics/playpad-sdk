#nullable enable
namespace ElympicsPlayPad.Web3.Data
{
    public readonly struct SmartContract
    {
        public string? ChainId { get; init; }
        public string Address { get; init; }
        public string ABI { get; init; }

    }
}
