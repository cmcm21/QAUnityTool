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
        private const string StartRecButtonName = "StartRecButton";
        private const string StopRecButtonName = "StopRecButton";
        private const string ClearRecButtonName = "ClearRecording";
        private const string SaveRecButtonName = "SaveButton";
        private const string LoadButtonName = "LoadButton";
        private const string PlayButtonName = "Playbutton";

        private VisualElement _root;
        private Button _instantiateButton;
        private Button _startRecButton;
        private Button _stopRecButton;
        private Button _clearRecButton;
        private Button _saveRecButton;
        private Button _loadButton;
        private Button _playButton;
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
        }

        private void SetupButtons()
        {
            _instantiateButton = _root.Q<Button>(InstantiateSnifferButton);            
            if(_instantiateButton == null)
                ElementError(InstantiateSnifferButton);
            else
                _instantiateButton.clicked += InstantiateButtonOnClicked;
             
            SetupRecordButtons();
            SetupPlaybackButtons();
        }

        private void SetupRecordButtons()
        {
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

             _clearRecButton = _root.Q<Button>(ClearRecButtonName);
             if(_clearRecButton == null)
                 ElementError(ClearRecButtonName);
             else
             {
                 _clearRecButton.clicked += ClearRecButtonOnClicked;
             }
 
             _saveRecButton = _root.Q<Button>(SaveRecButtonName);
             if(_saveRecButton == null)
                 ElementError(SaveRecButtonName);
             else
             {
                 _saveRecButton.SetEnabled(false);
                 _saveRecButton.clicked += SaveRecButtonOnClicked;
             }
        }


        private void SetupPlaybackButtons()
        {
             _loadButton = _root.Q<Button>(LoadButtonName);
             if(_loadButton == null)
                 ElementError(LoadButtonName);
             else
             {
                 _loadButton.clicked += LoadButtonOnClicked;
                 _loadButton.SetEnabled(false);
             }
 
             _playButton = _root.Q<Button>(PlayButtonName);
             if (_playButton == null)
                 ElementError(PlayButtonName);
             else
             {
                 _playButton.clicked += PlayButtonOnClicked;
                 _playButton.SetEnabled(false);
             }
        }

        private void StartRecButtonOnClicked()
        {
            if (_snifferCore == null) return;
            _startRecButton.SetEnabled(false);
            _stopRecButton.SetEnabled(true);
            _snifferCore.Record(); 
        }
        
        private void StopRecButtonOnClicked()
        {
            _snifferCore.Stop(); 
            _stopRecButton.SetEnabled(false);
            _startRecButton.SetEnabled(true);
            _saveRecButton.SetEnabled(true);
            _playButton.SetEnabled(true);
            
        }
        
        private void SaveRecButtonOnClicked()
        {
            var recordPathName = EditorUtility.SaveFilePanel(
                "Recording file", 
                SnifferDefinitions.RECORDINGS_PATH, 
                "record", 
                 GetRecordingFileExtenstion()
                );
            
            _snifferCore.Save(recordPathName);
            AssetDatabase.Refresh();
        }
         
        private void ClearRecButtonOnClicked()
        {
        }

        private void PlayButtonOnClicked()
        {
            _snifferCore.Play();
        }

        private void LoadButtonOnClicked()
        {
            var recordingFileName = EditorUtility.OpenFilePanel(
                "Select recording File",
                SnifferDefinitions.RECORDINGS_PATH,
                GetRecordingFileExtenstion()
            );
            
            _snifferCore.Load(recordingFileName);
            _playButton.SetEnabled(true);
        }


        private string GetRecordingFileExtenstion()
        {
            if (_snifferCore is { SysType: SnifferSettings.InputSystemType.OLD_INPUT })
                return "json";
            else if(_snifferCore is {SysType: SnifferSettings.InputSystemType.NEW_INPUT})
                return "inputtrace";
            return "";
        }

        private void InstantiateButtonOnClicked()
        {
            _instantiateButton.SetEnabled(false);
            _snifferCore = new SnifferCore();
            _snifferCore.Init();

            _snifferObjectField.value = GameObject.Find("SnifferObserver");
            _startRecButton.SetEnabled(true);
            _loadButton.SetEnabled(true);
        }


        private void ElementError(string elementName)
        {
            Debug.Log($"[{GetType()}]:: Element {elementName} doesn't found");
        }
    }
}