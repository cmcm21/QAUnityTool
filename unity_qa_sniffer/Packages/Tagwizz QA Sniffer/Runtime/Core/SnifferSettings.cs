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
      [Tooltip("If enabled, additional events will be recorded that demarcate frame boundaries. When replaying, this allows "
          + "spacing out input events across frames corresponding to the original distribution across frames when input was "
          + "recorded. If this is turned off, all input events will be queued in one block when replaying the trace.")]
      [SerializeField] private bool recordInputFrames;

      [Tooltip("If enabled, new devices will be created for captured events when replaying them. If disabled (default), "
          + "events will be queued as is and thus keep their original device ID.")]
      [SerializeField] private bool replayOnNewDevices;

      [Tooltip("If enabled, the system will try to simulate the original event timing on replay. This differs from replaying frame "
          + "by frame in that replay will try to compensate for differences in frame timings and redistribute events to frames that "
          + "more closely match the original timing. Note that this is not perfect and will not necessarily create a 1:1 match.")]
      [SerializeField] private bool simulateOriginalTimingOnReplay;

      [Tooltip("If enabled, only StateEvents and DeltaStateEvents will be captured.")]
      [SerializeField] private bool recordStateEventsOnly;

      [Header("Frame Recorder Settings")] 
      [SerializeField] private bool liveStreaming;
      [SerializeField] private int maxFramesPerRec;
      [SerializeField] private int frameRate;
      
      public SnifferObserver SnifferObserver => snifferObserver;
      public InputSystemType InputSystem => inputSystemType;
      public RecordingType RecordingOption => recordingOption;
      
      public bool LiveStreaming => liveStreaming;
      public int MaxFramesPerRec => maxFramesPerRec;
      public int FrameRate => frameRate;
      public bool RecordInputFrames => recordInputFrames;
      public bool ReplayOnNewDevices => replayOnNewDevices;
      public bool SimulateOriginalTimingOnReplay => simulateOriginalTimingOnReplay;
      public bool RecordStateEventOnly => recordStateEventsOnly;
   }
}
