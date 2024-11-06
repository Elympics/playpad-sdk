using Cysharp.Threading.Tasks;
using Elympics.Models.Authentication;
using ElympicsPlayPad.ExternalCommunicators;
using ElympicsPlayPad.ExternalCommunicators.Authentication;
using ElympicsPlayPad.ExternalCommunicators.Authentication.Models;
using ElympicsPlayPad.ExternalCommunicators.Web3.Wallet;
using NSubstitute;

namespace ElympicsPlayPad.Tests.PlayMode
{
    public static class ExternalCommunicatorComponentMocker
    {
        // public static void MockIExternalAuthenticatorAndSet(this PlayPadCommunicator communicator, AuthData authData, Capabilities mockCapabilities, string environment, string closestRegion)
        // {
        //     var authMock = Substitute.For<IExternalAuthenticator>();
        //     _ = authMock.InitializationMessage(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).Returns(UniTask.FromResult(new ExternalAuthData(authData, true, mockCapabilities, environment, closestRegion, null)));
        //     communicator.SetCustomExternalAuthenticator(authMock);
        // }
        //
        // public static void MockExternalWalletCommunicatorAndSet(this PlayPadCommunicator communicator, string address, string chainId)
        // {
        //     var walletMock = Substitute.For<IExternalWalletCommunicator>();
        //     walletMock.Connect().Returns(UniTask.FromResult(new ConnectionResponse()
        //     {
        //         address = address,
        //         chainId = chainId,
        //     }));
        //     communicator.SetCustomExternalWalletCommunicator(walletMock);
        // }
        //
        // public static void MockExternalWalletCommunicatorWithDisconnectionAndSet(this PlayPadCommunicator communicator, string address, string chainId)
        // {
        //     var walletMock = Substitute.For<IExternalWalletCommunicator>();
        //     IPlayPadEventListener listener = null;
        //     walletMock.When(x => x.SetPlayPadEventListener(Arg.Any<IPlayPadEventListener>())).Do(x =>
        //     {
        //         listener = x.Arg<IPlayPadEventListener>();
        //     });
        //
        //     walletMock.Connect().ReturnsForAnyArgs(x => UniTask.FromResult(new ConnectionResponse()
        //     {
        //         address = address,
        //         chainId = chainId,
        //     })).AndDoes(x => listener.OnWalletDisconnected());
        //     communicator.SetCustomExternalWalletCommunicator(walletMock);
        // }
        //
        // public static void MockExternalWalletCommunicatorWithConnectedAndSet(this PlayPadCommunicator communicator, string address, string chainId)
        // {
        //     var walletMock = Substitute.For<IExternalWalletCommunicator>();
        //     IPlayPadEventListener listener = null;
        //     walletMock.When(x => x.SetPlayPadEventListener(Arg.Any<IPlayPadEventListener>())).Do(x =>
        //     {
        //         listener = x.Arg<IPlayPadEventListener>();
        //     });
        //
        //     walletMock.Connect().ReturnsForAnyArgs(x => UniTask.FromResult(new ConnectionResponse()
        //     {
        //         address = address,
        //         chainId = chainId,
        //     })).AndDoes(x => listener.OnWalletConnected(address, chainId));
        //     communicator.SetCustomExternalWalletCommunicator(walletMock);
        // }
    }
}
