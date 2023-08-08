using TagwizzQASniffer.Core.InputSystem;
using UnityEngine;

namespace TagwizzQASniffer.Core.Recording
{
   public enum RecorderClipType
   {
      IDLE,
      EVENT_ACTION,
      CONTINUOUS_ACTION
   };

   [System.Serializable]
   public class RecorderClip
   {
      [SerializeField] public string type;
      [SerializeField] public InputData inputData;
      [SerializeField] public float timelineStart;
      [SerializeField] public float timelineEnd;
   }
}