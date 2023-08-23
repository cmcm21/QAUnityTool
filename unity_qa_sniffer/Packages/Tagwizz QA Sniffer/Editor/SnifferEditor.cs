using System.IO;
using TagwizzQASniffer.Core;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
namespace TagwizzQASniffer.Editor
{
    [InitializeOnLoad]
    public class SnifferEditor
    {
        static SnifferEditor()
        {
            CreateRecordingsDirectory(); 
            CreateSettingsFile();
            if(!Application.isPlaying)
                AssetDatabase.Refresh();
        }

        private static void CreateSettingsFile()
        {
            if (!File.Exists(SnifferEndPoints.SnifferSettingsPath))
            {
                Debug.Log($"[{typeof(SnifferEditor)}]::Asset : {SnifferEndPoints.SnifferSettingsPath} doesn't exists");
                SnifferSettings settings = ScriptableObject.CreateInstance<SnifferSettings>();
                Directory.CreateDirectory("Assets/Resources");
                AssetDatabase.CreateAsset(settings,SnifferEndPoints.SnifferSettingsPath);
                Debug.Log($"A[{typeof(SnifferEditor)}]::Asset: {SnifferEndPoints.SnifferSettingsPath} created");
             
            }
            else
            {
                Debug.Log($"[{typeof(SnifferEditor)}]::Asset: {SnifferEndPoints.SnifferSettingsPath} exists"); 
            }
         
        }

        private static void CreateRecordingsDirectory()
        {
            if (!Directory.Exists(SnifferEndPoints.RecordingsFilesDirectory))
            {
                Directory.CreateDirectory(SnifferEndPoints.RecordingsFilesDirectory);
                Debug.Log($"[{typeof(SnifferEditor)}]::Directory: {SnifferEndPoints.RecordingsFilesDirectory} created");
            }
            else
            {
                Debug.Log($"[{typeof(SnifferEditor)}]Directory: {SnifferEndPoints.RecordingsFilesDirectory} already exists");
            }
        }
    }

    public static class SnifferEndPoints
    {
        public static readonly string RecordingsFilesDirectory = "Assets/SnifferRecordings/";
        public static readonly string SnifferSettingsPath = "Assets/Resources/SnifferSettings.asset"; 
    }
}
#endif
