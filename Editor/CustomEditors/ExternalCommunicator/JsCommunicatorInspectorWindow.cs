using ElympicsPlayPad.ExternalCommunicators.WebCommunication.Js;
using UnityEditor;
using UnityEngine.UIElements;

namespace ElympicsPlayPad.Editor.ExternalCommunicator
{
    [CustomEditor(typeof(JsCommunicator))]
    public class JsCommunicatorInspectorWindow : UnityEditor.Editor
    {
        public VisualTreeAsset m_InspectorXML;
        private JsCommunicator _target;
        private TextField _jsonWebMessage;

        private string _tournamentJson;

        public override VisualElement CreateInspectorGUI()
        {
            var myInspector = new VisualElement();
            m_InspectorXML.CloneTree(myInspector);
            SetupButtonActions(myInspector);
            var quer = myInspector.Q<Foldout>("foldout");
            _jsonWebMessage = quer.Q<TextField>("json_web_message");
            _target = (JsCommunicator)serializedObject.targetObject;
            return myInspector;
        }
        private void SetupButtonActions(VisualElement root)
        {
            var buttons = root.Query<Button>();
            buttons.ForEach(RegisterCallbacks);
        }
        private void RegisterCallbacks(Button button)
        {
            if (button.name == "tournament_changed")
                button.RegisterCallback<ClickEvent>(OnAuthAndConnectClicked);
        }
        private void OnAuthAndConnectClicked(ClickEvent evt) => _target.HandleWebEvent(_jsonWebMessage.value);
    }
}
