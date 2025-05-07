namespace ElympicsPlayPad
{
    public static class UnityUtil
    {
#pragma warning disable IDE0025
        public static bool IsEditor
        {
            get
            {
#if UNITY_EDITOR
                return true;
#else
                return false;
#endif
            }
        }
#pragma warning restore IDE0025
    }
}
