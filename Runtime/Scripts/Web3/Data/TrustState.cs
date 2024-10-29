namespace ElympicsPlayPad.Web3.Data
{
    public struct TrustState
    {
        public float TotalAmount;
        public float AvailableAmount;

        public static TrustState Noop => new TrustState()
        {
            TotalAmount = 0.0f,
            AvailableAmount = 0.0f
        };
    }
}
