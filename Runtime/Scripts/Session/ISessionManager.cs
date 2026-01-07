namespace ElympicsPlayPad.Session
{
    public interface ISessionManager
    {
        SessionInfo? CurrentSession { get; }
    }
}
