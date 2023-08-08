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

      [SerializeField] private InputSystemType inputSystemType;
      [SerializeField] private LifeCycle lifeCycle;
      [SerializeField] private RecordingType recordingOption;
      public LifeCycle LifeCycle => lifeCycle;
      public InputSystemType InputSystem => inputSystemType;
      public RecordingType RecordingOption => recordingOption;

   }
}
