using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;


public class SnifferRecorderW : EditorWindow
{
    [MenuItem("Sniffer/Recorder")]
    public static void ShowExample()
    {
        SnifferRecorderW wnd = GetWindow<SnifferRecorderW>();
        wnd.titleContent = new GUIContent("Sniffer Recorder");
    }

    public void CreateGUI()
    {
        VisualElement root = rootVisualElement;

        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Core/Editor/SnifferEditorW.uxml");
        root.Add(visualTree.Instantiate());
        
        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Core/Editor/SnifferEditorW.uss");
        root.styleSheets.Add(styleSheet);
    }
}