using System.Threading;
using Elympics;
using Elympics.Cysharp.Threading.Tasks;
using Elympics.Models.Authentication;

namespace ElympicsPlayPad.Auth
{
    internal interface IAuthClient
    {
        UniTask<Result<AuthData, string>> AuthenticateWithClientSecret(string clientSecret, CancellationToken ct = default);
    }
}
