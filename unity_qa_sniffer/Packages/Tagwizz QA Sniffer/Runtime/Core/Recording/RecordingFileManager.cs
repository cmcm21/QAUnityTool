
using System.IO;
using UnityEngine;

namespace TagwizzQASniffer.Core.Recording
{
    public static class RecordingFileManager
    {
        public static void SaveToJson(RecordingData recordingData, string fileName = "")
        {
            var jsonfile = JsonUtility.ToJson(recordingData,true);
            File.WriteAllText(fileName,jsonfile); 
        }

        public static void OverrideFile(RecordingData recordingData, string fileName = "")
        {
            
        }
        
        private static string GetFilePath(string recordingName)
        {
            int counter = 1;
            string recordingPath =  $"{SnifferDefinitions.RECORDINGS_PATH}{recordingName}.json";
            while (File.Exists(recordingPath))
            {
                recordingPath = $"{SnifferDefinitions.RECORDINGS_PATH}{recordingName}_{counter}.json";
                counter++;
            }
            return recordingPath;
        }
    }
}
