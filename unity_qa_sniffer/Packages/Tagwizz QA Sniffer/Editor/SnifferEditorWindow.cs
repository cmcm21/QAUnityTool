using TagwizzQASniffer.Core;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Button = UnityEngine.UIElements.Button;

namespace TagwizzQASniffer.Editor
{
    public class SnifferEditorWindow : EditorWindow
    {
        private const string UxmlId = "4507d42a19f77ad448df2608e020b92c";
        private const string UssId = "072e9d94ff16a0b42ab2934590ce5ffb";
        
        private const string InstantiateSnifferButton = "InstantiateSnifferButton";
        private const string SnifferObjectFieldName = "SnifferContainerObjField";
        private const string RecordingTextFieldName = "RecordingTextField";
        private const string StartRecButtonName = "StartRecButton";
        private const string StopRecButtonName = "StopRecButton";

        private VisualElement _root;
        private Button _instantiateButton;
        private Button _startRecButton;
        private Button _stopRecButton;
        private TextField _recordingTextField;
        private ObjectField _snifferObjectField;
        private SnifferCore _snifferCore;

        [MenuItem("Sniffer/Recorder")]
        public static void ShowWindow()
        {
            if (!Application.isPlaying)
            {
                WarningMsg();
                return;
            }
            SnifferEditorWindow wnd = GetWindow<SnifferEditorWindow>();
            wnd.titleContent = new GUIContent("Sniffer Window");
        }

        private static void WarningMsg()
        {
            Debug.LogWarning("The game should be playing to use sniffer");
        }

        public void CreateGUI()
        {
            _root = rootVisualElement;

            var uxmlPath = AssetDatabase.GUIDToAssetPath(UxmlId);
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlPath);
            if(visualTree == null)
                Debug.LogError($"[{GetType()}]:: uxml {uxmlPath} wasn't loaded correctly");
            else
                _root.Add(visualTree.Instantiate());

            var ussPath = AssetDatabase.GUIDToAssetPath(UssId); 
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(ussPath);
            if(styleSheet == null)
                Debug.LogError($"[{GetType()}]:: uss {ussPath} wasn't loaded correctly");
            else
                _root.styleSheets.Add(styleSheet);
            
            Setup();
            SetupButtons();
        }

        private void Setup()
        {
            _snifferObjectField = _root.Q<ObjectField>(SnifferObjectFieldName);
            if (_snifferObjectField == null)
                ElementError(SnifferObjectFieldName);
            else
            {
                _snifferObjectField.objectType = typeof(GameObject);
                _snifferObjectField.SetEnabled(false);
            }

            _recordingTextField = _root.Q<TextField>(RecordingTextFieldName);
            if(_recordingTextField == null)
                ElementError(RecordingTextFieldName);
            else
                _recordingTextField.value = string.Empty;
        }

        private void SetupButtons()
        {
            _instantiateButton = _root.Q<Button>(InstantiateSnifferButton);            
            if(_instantiateButton == null)
                ElementError(InstantiateSnifferButton);
            else
                _instantiateButton.clicked += InstantiateButtonOnClicked;
             
            _startRecButton = _root.Q<Button>(StartRecButtonName);
            if(_startRecButton == null)
                ElementError(SnifferObjectFieldName);
            else
            {
                _startRecButton.clicked += StartRecButtonOnClicked;
                _startRecButton.SetEnabled(false);
            }

            _stopRecButton = _root.Q<Button>(StopRecButtonName);
            if(_stopRecButton == null)
                ElementError(StopRecButtonName);
            else
            {
                _stopRecButton.clicked += StopRecButtonOnClicked;
                _stopRecButton.SetEnabled(false);
            }
        }

        private void InstantiateButtonOnClicked()
        {
            _instantiateButton.SetEnabled(false);
            //Create sniffer and start it    
            _snifferCore = new SnifferCore();
            _snifferCore.Init();

            _snifferObjectField.value = GameObject.Find("LifeCycle");
            _startRecButton.SetEnabled(true);
        }

        private void StopRecButtonOnClicked()
        {
            _snifferCore.Stop(); 
            _stopRecButton.SetEnabled(false);
            _startRecButton.SetEnabled(true);
            
            _snifferCore.SaveRecord(_recordingTextField.value);
            AssetDatabase.Refresh();
        }

        private void StartRecButtonOnClicked()
        {
            if (_snifferCore == null) return;
            _startRecButton.SetEnabled(false);
            _stopRecButton.SetEnabled(true);
            _snifferCore.Record(); 
        }

        private void ElementError(string elementName)
        {
            Debug.Log($"[{GetType()}]:: Element {elementName} doesn't found");
        }
    }
}