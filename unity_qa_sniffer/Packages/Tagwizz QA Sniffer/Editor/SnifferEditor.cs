using System.IO;
using TagwizzQASniffer.Core;
using UnityEditor;
using UnityEngine;

namespace TagwizzQASniffer.Editor
{
    [InitializeOnLoad]
    public class SnifferEditor
    {
        static SnifferEditor()
        {
            CreateSettingFile();
            CreateRecordingsDirectory(); 
            AssetDatabase.Refresh();
        }

        private static void CreateSettingFile()
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
                Debug.Log($"[{typeof(SnifferEditor)}]::Directory: {SnifferEndPoints.SnifferSettingsPath} created");
            }
            else
            {
                Debug.Log($"[{typeof(SnifferEditor)}]Directory: {SnifferEndPoints.SnifferSettingsPath} already exists");
            }
        }
    }

    internal static class SnifferEndPoints
    {
        public static readonly string RecordingsFilesDirectory = "Assets/SnifferRecordings/";
        public static readonly string SnifferSettingsPath = "Assets/Resources/SnifferSettings.asset"; 
    }

}
