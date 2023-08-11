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
      [FormerlySerializedAs("lifeCycle")] [SerializeField] private SnifferObserver snifferObserver;
      [SerializeField] private RecordingType recordingOption;
      public SnifferObserver SnifferObserver => snifferObserver;
      public InputSystemType InputSystem => inputSystemType;
      public RecordingType RecordingOption => recordingOption;

   }
}
