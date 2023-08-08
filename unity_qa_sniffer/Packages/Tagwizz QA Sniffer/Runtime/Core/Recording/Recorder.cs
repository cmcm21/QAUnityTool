using System;
using TagwizzQASniffer.Core.InputSystem;
using TagwizzQASniffer.Core.InputSystem.NewSystemInput;
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
                _inputSystem = new OldSystemInput();
            else
                _inputSystem = new NewSystemInput();
            
            _inputSystem.Init();
            _inputSystem.InputEnded += InputSystemOnInputEnded;
            _inputSystem.InputStarted += InputSystemOnInputStarted;

            _state = RecordingState.IDLE;
        }

        private void InputSystemOnInputStarted(InputData inputData)
        {
            _timeline.ClipStarted(inputData);
        }

        private void InputSystemOnInputEnded(InputData inputData)
        {
            _timeline.ClipFinished(inputData);
        }

        public void StartRec(string recordingName = "")
        {
            _state = RecordingState.RECORDING;
            _timeline = new RecorderTimeline(recordingName);
        }

        public void StopRec()
        {
            _state = RecordingState.STOP;
            _inputSystem.Stop();
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
                _timeline.Update();
                _inputSystem.ReadInputs();
            }
        }

        public RecordingData GetRecordingData()
        {
            if(_state == RecordingState.STOP || _state == RecordingState.IDLE)
              return _timeline.Export();
            
            return null;
        } 
    }
}
