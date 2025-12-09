using Cysharp.Threading.Tasks;
using Elympics.Models.Authentication;
namespace ElympicsPlayPad.Session
{
    public interface ISessionManagerAuthProvider
    {
        public UniTask Authenticate(AuthData cachedData, string region, bool autoRetry);
        public void SignOut();
        bool IsAuthenticated { get; }
    }
}
