#nullable enable
using System;
using Nethereum.Signer;
using Plugins.Elympics.Plugins.ParrelSync;
using UnityEngine;

namespace ElympicsPlayPad.ExternalCommunicators.Web3.Wallet
{
    [CreateAssetMenu(fileName = "WalletConfig", menuName = "Configs/Standalone/WalletConfig")]
    public class StandaloneWalletConfig : ScriptableObject
    {
        private const string EthPrivateKeyPlayerPrefsKeyBase = "Elympics/EthPrivateKey";
        private static string EthPrivateKeyPlayerPrefsKey =>
                ElympicsClonesManager.IsClone()
                ? $"{EthPrivateKeyPlayerPrefsKeyBase}_clone_{ElympicsClonesManager.GetCloneNumber()}"
                : EthPrivateKeyPlayerPrefsKeyBase;

        [Header("Wallet")]
        [SerializeField] private bool isWalletAvailable;

        [SerializeField] private string chainId;

        [Header("Connection")]
        [SerializeField] private bool acceptsConnection;

        [Header("Signature")]
        [SerializeField] private bool shouldConfirmSignatures;

        [Header("Blockchain")]
        [SerializeField] private bool shouldSignTransactions;
        [SerializeField] private bool shouldSendTransactions;
        [SerializeField] private bool shouldGetValues;

        [Header("Delay")]
        [SerializeField] private int connectionDelay;
        [SerializeField] private int signatureDelay;

        private EthECKey _walletKey;

        public bool IsWalletAvailable => isWalletAvailable;
        public string ChainId => chainId;
        public bool AcceptsConnection => acceptsConnection;
        public bool ShouldConfirmSignatures => shouldConfirmSignatures;
        public bool ShouldSignTransactions => shouldSignTransactions;
        public bool ShouldSendTransactions => shouldSendTransactions;
        public bool ShouldGetValues => shouldGetValues;
        public int ConnectionDelay => connectionDelay;
        public int SignatureDelay => signatureDelay;

        public EthECKey? GetWalletKey()
        {
            if (_walletKey is null)
                ReinitializeUsedKey();
            return _walletKey;
        }

        private void ReinitializeUsedKey()
        {
            var args = Environment.GetCommandLineArgs();

            for (var i = 0; i < args.Length; i++)
            {
                if (!args[i].Contains("-privateKey"))
                    continue;
                try
                {
                    _walletKey = new EthECKey(args[i + 1]);
                    return;

                }
                catch (Exception)
                {
                    Debug.LogError($"Invalid privateKey from console arguments.");
                    break;
                }
            }

            var key = EthPrivateKeyPlayerPrefsKey;
            if (!PlayerPrefs.HasKey(key))
            {
                PlayerPrefs.SetString(key, CreateNewEthPrivateKey());
                PlayerPrefs.Save();
            }

            _walletKey = new EthECKey(PlayerPrefs.GetString(key));
        }

        private static string CreateNewEthPrivateKey() => EthECKey.GenerateKey().GetPrivateKey();


        private void InvokeRelogin()
        {
            //WalletWebGLEventsReceiver.WalletAccountsChanged();
        }
    }
}
