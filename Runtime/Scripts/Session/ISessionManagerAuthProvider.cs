using Cysharp.Threading.Tasks;
using Elympics.Models.Authentication;

namespace ElympicsPlayPad.Session
{
    public interface ISessionManagerAuthProvider
    {
        UniTask Authenticate(AuthData cachedData, string region, bool autoRetry);
        void SignOut();
        bool IsAuthenticated { get; }
    }
}
