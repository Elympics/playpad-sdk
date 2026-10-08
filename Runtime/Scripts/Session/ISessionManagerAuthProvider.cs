using Cysharp.Threading.Tasks;
using Elympics;
using Elympics.Models.Authentication;

namespace ElympicsPlayPad.Session
{
    public interface ISessionManagerAuthProvider
    {
        UniTask Authenticate(AuthData cachedData, string region = ElympicsRegions.Default, bool autoRetry = false);
        void SignOut();
        bool IsAuthenticated { get; }
    }
}
