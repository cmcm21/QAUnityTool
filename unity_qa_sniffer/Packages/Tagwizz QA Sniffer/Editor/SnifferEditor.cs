using System.IO;
using TagwizzQASniffer.Core;
using UnityEditor;
using UnityEngine;

namespace TagwizzQASniffer
{
    [InitializeOnLoad]
    public class SnifferEditor
    {
        public static readonly string Path = "Assets/Resources/SnifferSettings.asset"; 
        static SnifferEditor()
        {
            CreateSettingFile();
        
        }

        private static void CreateSettingFile()
        {
            if (!File.Exists(Path))
            {
                Debug.Log($"Asset : {Path} doesn't exists");
                SnifferSettings settings = ScriptableObject.CreateInstance<SnifferSettings>();
                Directory.CreateDirectory("Assets/Resources");
                AssetDatabase.CreateAsset(settings,Path);
                Debug.Log($"Asset: {Path} created");
             
            }
            else
            {
                Debug.Log($"Asset: {Path} exists"); 
            }
         
        }
    }
}
