using TagwizzQASniffer.Core;
using TagwizzQASniffer.Core.InputSystem;
using UnityEngine;
using UnityEngine.Serialization;

namespace TagwizzQASniffer.Core
{
   public class SnifferSettings: ScriptableObject
   {
      public enum InputSystemType
      {
         NEW_INPUT,
         OLD_INPUT
      };

      public enum RecordingType
      {
         RECORDING_EVERYTHING,
         CUSTOM_RECORDING
      }

      
      [Header("Input Recorder Settings")]
      [SerializeField] private InputSystemType inputSystemType;
      [SerializeField] private SnifferObserver snifferObserver;
      [SerializeField] private RecordingType recordingOption;

      [Header("Frame Recorder Settings")] 
      [SerializeField] private bool recordFrames;
      [SerializeField] private int maxFramesPerRec;
      [SerializeField] private int frameRate;
      
      public SnifferObserver SnifferObserver => snifferObserver;
      public InputSystemType InputSystem => inputSystemType;
      public RecordingType RecordingOption => recordingOption;
      public int MaxFramesPerRec => maxFramesPerRec;
      public bool RecordFrames => recordFrames;
      public int FrameRate => frameRate;
   }
}
