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

        private void InputSystemOnInputStarted(InputData inputData)
        {
            _timeline.ClipStarted(inputData);
        }

        private void InputSystemOnInputEnded(InputData inputData)
        {
            _timeline.ClipFinished(inputData);
        }

        public int GetRecLenght() => 0;

        public int GetRecPosition() => 0;

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

        public void Play() {}
        public void StopPlay() { }

        public void LoadFromFile(string fileName) { }
        
        public void Clear(){}
        public void Pause() { }
    }
}
