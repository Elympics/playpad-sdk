namespace ElympicsPlayPad.Web3.Data.Providers
{
    public interface ITokenAddressProvider
    {
        string GetAddress();
        int GetChainId();
    }
}
