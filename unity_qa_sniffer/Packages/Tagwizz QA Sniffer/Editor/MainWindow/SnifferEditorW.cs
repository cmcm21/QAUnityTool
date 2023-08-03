using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using TagwizzQASniffer.Core;

namespace TagwizzQASniffer.Editor.MainWindow
{
    public class SnifferEditorW : EditorWindow
    {
        private const string UxmlPath = "Packages/Tagwizz QA Sniffer/Editor/MainWindow/SnifferEditorW.uxml";
        private const string USSPath = "Packages/Tagwizz QA Sniffer/Editor/MainWindow/SnifferEditorW.uss";
        
        [MenuItem("Sniffer/Recorder/Start")]
        public static void StartRecording()
        {
            if (!Application.isPlaying)
            {
                WarningMsg();
                return;
            }
            /*
            SnifferEditorW wnd = GetWindow<SnifferEditorW>();
            wnd.titleContent = new GUIContent("Sniffer Recorder");
            */
        }

        [MenuItem("Sniffer/Recorder/Stop")]
        public static void StopRecording()
        {
            if (!Application.isPlaying)
            {
                WarningMsg();
                return;
            }
        }

        private static void WarningMsg()
        {
            Debug.Log("The game should be playing to use sniffer");
        }

        public void MyFunction()
        {
            Debug.Log($"[{GetType()}]::Creating window");
            VisualElement root = rootVisualElement;

            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/Tagwizz QA Sniffer/Editor/MainWindow/SnifferEditorW.uxml");
            if(visualTree == null)
                Debug.LogError($"[{GetType()}]:: uxml {UxmlPath} wasn't loaded correctly");
            else
                root.Add(visualTree.Instantiate());
            
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(USSPath);
            if(styleSheet == null)
                Debug.LogError($"[{GetType()}]:: uss {USSPath} wasn't loaded correctly");
            else
                root.styleSheets.Add(styleSheet);
        }
    }
}