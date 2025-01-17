using System;
using TagwizzQASniffer.Core.InputSystem;
using TagwizzQASniffer.Core.InputSystem.OldSystemInput;

namespace TagwizzQASniffer.Core.Recording
{
    public enum RecordingState
    {
        RECORDING,
        IDLE,
        STOP
    };
    public class OldInpSysRecorder: IRecorder
    {
        private readonly OldInputSystem _inputSystem;
        private RecordingState _state;
        private RecorderTimeline _timeline;
        
        public OldInpSysRecorder()
        {
            _inputSystem = new OldInputSystem();
            _inputSystem.InputEnded += InputSystemOnInputEnded;
            _inputSystem.InputStarted += InputSystemOnInputStarted;

            _state = RecordingState.IDLE;
        }
        
        //
        bool IRecorder.Subscribe(IRecorderListener listener) {
            throw new NotImplementedException();
        }

        void IRecorder.Unsubscribe(IRecorderListener listener) {
            throw new NotImplementedException();
        }

        private void InputSystemOnInputStarted(InputData inputData)
        {
            _timeline.ClipStarted(inputData);
        }

        private void InputSystemOnInputEnded(InputData inputData)
        {
            _timeline.ClipFinished(inputData);
        }

        public event Action OnRecordStarted;
        public event Action OnRecordFinished;
        public event Action OnReplayStarted;
        public event Action OnReplayFinished;

        public int GetReplayPosition() => 0;

        public int GetRecordingLength() => 0;

        public void StartRec()
        {
            _state = RecordingState.RECORDING;
            _inputSystem.Init();
            _timeline = new RecorderTimeline();
        }

        public void StopRec()
        {
            _state = RecordingState.STOP;
            _inputSystem.Stop();
        }

        public void OnAwake() { }

        public void OnStart() { }

        public void OnUpdate()
        {
            if (_state == RecordingState.RECORDING)
            {
                _timeline.Update();
                _inputSystem.ReadInputs();
            }
        }

        public void OnEnabled() { }

        public void OnDisabled() { }

        public void OnDestroy() { }

        public void SaveToFile(string fileName)
        {
            var recData = _timeline.Export();
            RecordingFileManager.SaveToJson(recData,fileName);
        }

        public void Replay() {}
        public void StopPlay() { }
        public void ReplayOneStep() { }

        public void LoadFromFile(string fileName) { }
        
        public void Clear(){}
        public void Pause() { }
    }
}
