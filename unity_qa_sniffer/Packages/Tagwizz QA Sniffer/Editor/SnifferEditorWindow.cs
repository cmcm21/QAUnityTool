using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using TagwizzQASniffer.Core;

namespace TagwizzQASniffer.Editor.MainWindow
{
    public class SnifferEditorWindow : EditorWindow
    {
        private const string UxmlId = "4507d42a19f77ad448df2608e020b92c";
        private const string UssId = "072e9d94ff16a0b42ab2934590ce5ffb";

        [MenuItem("Sniffer/Recorder")]
        public static void ShowWindow()
        {
            SnifferEditorWindow wnd = GetWindow<SnifferEditorWindow>();
            wnd.titleContent = new GUIContent("Sniffer Window");
        }

        private static void WarningMsg()
        {
            Debug.Log("The game should be playing to use sniffer");
        }

        public void MyFunction()
        {
            Debug.Log($"[{GetType()}]::Creating window");
            VisualElement root = rootVisualElement;

            var uxmlPath = AssetDatabase.GUIDToAssetPath(UxmlId);
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlPath);
            if(visualTree == null)
                Debug.LogError($"[{GetType()}]:: uxml {uxmlPath} wasn't loaded correctly");
            else
                root.Add(visualTree.Instantiate());

            var ussPath = AssetDatabase.GUIDToAssetPath(UssId); 
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(ussPath);
            if(styleSheet == null)
                Debug.LogError($"[{GetType()}]:: uss {ussPath} wasn't loaded correctly");
            else
                root.styleSheets.Add(styleSheet);
        }
    }
}