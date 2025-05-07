using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using Elympics;
using ElympicsPlayPad.Editor;
using ElympicsPlayPad.Editor.ExtensionProtocol;
using ElympicsPlayPad.ExternalCommunicators.WebCommunication.Js.Extension;
using ElympicsPlayPad.ExternalCommunicators.WebCommunication.Js.Extension.Response;
using ElympicsPlayPad.Protocol;
using ElympicsPlayPad.Utility;
using UnityEngine;

namespace ElympicsPlayPad.ExternalCommunicators.WebCommunication.Js
{
#if UNITY_EDITOR
    internal class ExtensionPlayPadCommunicator : IPlayPadCommunicator
    {
        public event Action<string> ResponseMessageReceived;
        public event Action<string> WebMessageReceived;

        private const string MinSupportedExtensionVersion = "0.1.0";
        private const string ExtensionProtocolVersion = "0.1.0";
        private readonly Version _supportedExtensionVersion;
        private CurrentAuthenticationProcess? _currentAuthenticationProcess;
        private readonly Queue<ExtensionRequest> _requestQueue = new();
        private readonly CancellationTokenSource _cancellationTokenSource;

        public ExtensionPlayPadCommunicator()
        {
            _supportedExtensionVersion = new Version(MinSupportedExtensionVersion);
            _cancellationTokenSource = new CancellationTokenSource();
            PlayPadExtensionConnection.MessageReceived += OnMessageReceived;
            if (PlayPadExtensionConnection.IsConnected is false)
                try
                {
                    PlayPadExtensionConnection.ConnectToExtension();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    StartDroppingMessages(_cancellationTokenSource.Token).Forget();
                    return;
                }

            var config = ElympicsConfig.LoadCurrentElympicsGameConfig();
            if (!config)
                throw new ElympicsException("Couldn't load game config");
            var jwt = ElympicsConfig.AuthToken;
            if (string.IsNullOrEmpty(jwt))
                throw new ElympicsException("Please login to your Elympics account before connecting to PlayPad extension.");
            var gameId = config.GameId;
            var gameVersion = config.GameVersion;
            var gameName = config.GameName;
            if (PlayPadExtensionAuthentication.IsAuthenticated(gameId, gameVersion, jwt))
            {
                Debug.Log("Already authenticated with PlayPad extension using cached data.");
                StartRequestDispatcher(_cancellationTokenSource.Token).Forget();
                return;
            }
            Authenticate(gameId, gameVersion, gameName, jwt, PlayPadMessagingSystem.ProtocolVersion, ExtensionProtocolVersion);
        }

        private void Authenticate(
            string gameId,
            string gameVersion,
            string gameName,
            string jwt,
            string playpadProtocolVersion,
            string extensionProtocolVersion)
        {
            Debug.Log("Starting PlayPad extension authentication...");

            var handshake = new ExtensionHandshakeRequest
            {
                jwt = jwt,
                gameId = gameId,
                gameVersion = gameVersion,
                gameName = gameName,
                playpadSdkVersion = PlayPadSdkVersionRetriever.GetVersionStringFromAssembly(),
                playpadProtocolVersion = playpadProtocolVersion,
                extensionProtocolVersion = extensionProtocolVersion
            };

            var message = new ExtensionRequest
            {
                type = ExtensionMessageTypes.ExtensionHandshake,
                message = JsonUtility.ToJson(handshake),
            };

            _currentAuthenticationProcess = new CurrentAuthenticationProcess
            {
                GameId = gameId,
                GameVersion = gameVersion,
                Jwt = jwt
            };
            PlayPadExtensionConnection.SendProtocolMessage(message);
        }

        private async UniTask StartRequestDispatcher(CancellationToken cts)
        {
            Debug.Log("Starting extension request dispatcher.");
            while (true)
            {
                if (cts.IsCancellationRequested)
                    break;

                if (PlayPadExtensionConnection.IsConnected
                    && _requestQueue.Count > 0)
                    while (_requestQueue.Count > 0)
                    {
                        var message = _requestQueue.Dequeue();
                        PlayPadExtensionConnection.SendProtocolMessage(message);
                    }
                await UniTask.Yield();
            }
        }

        private async UniTask StartDroppingMessages(CancellationToken cts)
        {
            Debug.Log("Starting extension request dropper.");
            while (true)
            {
                if (cts.IsCancellationRequested)
                    break;

                if (PlayPadExtensionConnection.IsConnected is false)
                    if (_requestQueue.Count > 0)
                    {
                        Debug.LogWarning($"Dropping {_requestQueue.Count} queued extension messages due to extension connection failure. Use mocking in the editor to test PlayPad integration without the extension.");
                        _requestQueue.Clear();
                    }
                await UniTask.Yield();
            }
        }

        private void OnMessageReceived(byte[] data)
        {
            var message = Encoding.UTF8.GetString(data);
            var deserialized = JsonUtility.FromJson<ExtensionMessage>(message);
            if (deserialized.errorCode != 0 || !string.IsNullOrEmpty(deserialized.errorMessage))
            {
                Debug.LogError($"Extension protocol Error({deserialized.errorCode}) : {ErrorMessageMapper(deserialized.errorCode, deserialized.errorMessage)}");
                StartDroppingMessages(_cancellationTokenSource.Token).Forget();
                return;
            }
            switch (deserialized.type)
            {
                case ExtensionMessageTypes.ExtensionHandshake:

                    if (_currentAuthenticationProcess != null)
                    {
                        var extensionResponse = JsonUtility.FromJson<ExtensionHandshakeResponse>(deserialized.response);
                        var extensionVersion = new Version(extensionResponse.extensionVersion);
                        if (_supportedExtensionVersion != extensionVersion)
                        {
                            Debug.LogError(
                                $"Unsupported PlayPad Extension version detected: {extensionVersion}. Supported version: {_supportedExtensionVersion}. Please update the PlayPad SDK or PlayPad Extension.");
                            _currentAuthenticationProcess = null;
                            StartDroppingMessages(_cancellationTokenSource.Token).Forget();
                            return;
                        }

                        PlayPadExtensionAuthentication.Authenticate(_currentAuthenticationProcess.Value.GameId,
                            _currentAuthenticationProcess.Value.GameVersion,
                            _currentAuthenticationProcess.Value.Jwt);
                        _currentAuthenticationProcess = null;
                        StartRequestDispatcher(_cancellationTokenSource.Token).Forget();
                        Debug.Log("PlayPad Extension authentication completed.");
                    }
                    else
                        Debug.LogError("Unity did not requested authentication.");
                    break;
                case ExtensionMessageTypes.HandleWebMessageProtocol:
                    WebMessageReceived?.Invoke(deserialized.response);
                    break;
                case ExtensionMessageTypes.HandleResponseMessageProtocol:
                    ResponseMessageReceived?.Invoke(deserialized.response);
                    break;
                default:
                    Debug.LogError($"Unknown message type {deserialized.type}.");
                    break;
            }
        }

        private static string ErrorMessageMapper(int errorCode, string extensionErrorMessage)
        {
            return (ExtensionErrorCodes)errorCode switch
            {
                ExtensionErrorCodes.NotAuthenticated => "Unauthorized. Login onto developer account using Elympics SDK \"Tools/Elympics/Manage Games\"",
                _ => extensionErrorMessage
            };

        }

        public void SendRequestMessage(string messageType, string jsonMessage)
        {
            var type = messageType switch
            {
                PlayPadHandlers.VoidMessage => ExtensionMessageTypes.HandleVoidMessageProtocol,
                PlayPadHandlers.HandleMessage => ExtensionMessageTypes.RequestMessageProtocol,
                _ => throw new ArgumentOutOfRangeException(nameof(messageType), messageType, null)
            };
            var extensionMessage = new ExtensionRequest
            {
                type = type,
                message = jsonMessage
            };
            _requestQueue.Enqueue(extensionMessage);
        }
        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
            PlayPadExtensionConnection.MessageReceived -= OnMessageReceived;
            if (PlayPadExtensionConnection.IsConnected)
                PlayPadExtensionConnection.DisconnectFromExtension();
        }

        private readonly struct CurrentAuthenticationProcess
        {
            public string GameId { get; init; }
            public string GameVersion { get; init; }
            public string Jwt { get; init; }
        }
    }
#endif
}
