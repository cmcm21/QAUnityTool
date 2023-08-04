using System;
using TagwizzQASniffer.Core.InputSystem;
using TagwizzQASniffer.Core.InputSystem.OldSystemInput;
using UnityEngine;

namespace TagwizzQASniffer.Core.Recording
{
    public enum RecordingState
    {
        RECORDING,
        IDLE,
        STOP
    };
    public class Recorder: ILifeCycleSubscriber
    {
        private readonly ISnifferInputSystem _inputSystem;
        private RecordingState _state;
        private RecorderTimeline _timeline;
        

        public Recorder(Type inputType)
        {
            if (inputType == typeof(OldSystemInput))
            {
                _inputSystem = new OldSystemInput();
                _inputSystem.Init();
            }

            if (_inputSystem != null)
            {
                _inputSystem.InputEnded += InputSystemOnInputEnded;
                _inputSystem.InputStarted += InputSystemOnInputStarted;
            }

            _state = RecordingState.IDLE;
        }

        private void InputSystemOnInputStarted(InputData inputData)
        {
            _timeline.InputStarted(inputData);
        }

        private void InputSystemOnInputEnded(InputData inputData)
        {
            _timeline.InputFinished(inputData);
        }

        public void StartRec(string recordingName = "")
        {
            _state = RecordingState.RECORDING;
            _timeline = new RecorderTimeline(recordingName);
        }

        public void StopRec()
        {
            _state = RecordingState.STOP;
            Save();
        }

        public void OnAwake()
        {
        }

        public void OnStart()
        {
        }

        public void OnUpdate()
        {
            if (_state == RecordingState.RECORDING)
            {
                _inputSystem.ReadInputs();
                _timeline.Update();
            }
        }

        public void Save()
        {
            var recordingData = _timeline.Export();
            var jsonfile = JsonUtility.ToJson(recordingData);
            System.IO.File.WriteAllText(GetFilePath(recordingData.timelineName),jsonfile); 
        }

        private string GetFilePath(string recordingName)
        {
            return $"Assets/SnifferRecordings/{recordingName}.json";
        }
    }
}
