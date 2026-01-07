#nullable enable
using ElympicsPlayPad.ExternalCommunicators.Authentication;
using ElympicsPlayPad.ExternalCommunicators.GameStatus;
using ElympicsPlayPad.ExternalCommunicators.Leaderboard;
using ElympicsPlayPad.ExternalCommunicators.Tournament;
using ElympicsPlayPad.ExternalCommunicators.Ui;
using ElympicsPlayPad.ExternalCommunicators.VirtualDeposit;
using ElympicsPlayPad.ExternalCommunicators.Web3.Erc20SmartContract;
using ElympicsPlayPad.ExternalCommunicators.Web3.NFT;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace ElympicsPlayPad.Editor.MockConfiguration
{
    [CustomEditor(typeof(PlayPadMockConfiguration))]
    public class PlayPadMockConfigurationEditor : UnityEditor.Editor
    {
        [SerializeField] private VisualTreeAsset? m_InspectorXML;

        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();

            // Load UXML
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                "Packages/com.daftmobile.elympics-playpad/Editor/CustomEditors/MockConfiguration/Uxml/PlayPadMockConfiguration.uxml");

            if (visualTree != null)
            {
                visualTree.CloneTree(root);
                BindFields(root);
            }
            else
            {
                Debug.LogError("Could not load UXML file for PlayPadMockConfiguration inspector");
                // Fallback to default inspector
                return base.CreateInspectorGUI();
            }

            return root;
        }

        private void BindFields(VisualElement root)
        {
            // Bind Authentication Communicator
            BindCommunicatorRow<CustomStandaloneAuthenticationCommunicatorBase>(
                root,
                "customAuthenticatorCommunicator",
                "useStandaloneAuthenticationCommunicator");

            // Bind Leaderboard Communicator
            BindCommunicatorRow<CustomStandaloneLeaderboardCommunicatorBase>(
                root,
                "customLeaderboardCommunicator",
                "useCustomStandaloneLeaderboardCommunicator");

            // Bind Tournament Communicator
            BindCommunicatorRow<CustomStandaloneTournamentCommunicatorBase>(
                root,
                "customTournamentCommunicator",
                "useCustomStandaloneTournamentCommunicator");

            // Bind Game Status Communicator
            BindCommunicatorRow<CustomStandaloneGameStatusCommunicatorBase>(
                root,
                "customGameStatusCommunicator",
                "useCustomStandaloneGameStatusCommunicator");

            // Bind External UI Communicator
            BindCommunicatorRow<CustomStandaloneExternalUiCommunicatorBase>(
                root,
                "customExternalUiCommunicator",
                "useCustomStandaloneExternalUiCommunicator");

            // Bind ERC20 Smart Contract Communicator
            BindCommunicatorRow<CustomStandaloneErc20SmartContractCommunicatorBase>(
                root,
                "customErc20SmartContractCommunicator",
                "useCustomStandaloneErc20SmartContractCommunicator");

            // Bind BlockChain Currency Communicator
            BindCommunicatorRow<CustomStandaloneBlockChainCurrencyCommunicatorBase>(
                root,
                "customBlockChainCurrencyCommunicator",
                "useCustomStandaloneBlockChainCurrencyCommunicator");

            // Bind TON NFT Communicator
            BindCommunicatorRow<CustomTonNftExternalCommunicator>(
                root,
                "customTonNftExternalCommunicator",
                "useCustomTonNftExternalCommunicator");

            // Bind EVM Communicator
            BindCommunicatorRow<CustomEvmExternalCommunicator>(
                root,
                "customEvmExternalCommunicator",
                "useCustomEvmExternalCommunicator");
        }

        private void BindCommunicatorRow<T>(VisualElement root, string objectFieldName, string toggleName) where T : Object
        {
            var objectField = root.Q<ObjectField>(objectFieldName);
            var toggle = root.Q<Toggle>(toggleName);

            if (objectField != null)
            {
                objectField.objectType = typeof(T);
                objectField.BindProperty(serializedObject.FindProperty(objectFieldName));
                // Set label width to ensure consistent layout
                objectField.labelElement.style.minWidth = 80;
                objectField.labelElement.style.maxWidth = 120;
            }

            if (toggle != null)
            {
                toggle.BindProperty(serializedObject.FindProperty(toggleName));
                // Ensure toggle doesn't expand beyond needed size
                toggle.style.flexShrink = 0;
            }
        }
    }
}
