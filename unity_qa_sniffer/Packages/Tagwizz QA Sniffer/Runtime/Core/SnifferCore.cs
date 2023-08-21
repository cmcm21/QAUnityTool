using TagwizzQASniffer.Core.Recording;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TagwizzQASniffer.Core 
{
    public enum SnifferState {RECORDING,IDLE,PLAYING_BACK}
    public class SnifferCore
    {
        private SnifferState _state;
        public SnifferState State => _state;

        private IRecorder _recorder;
        public IRecorder Recorder => _recorder;
        private SnifferSettings _snifferSettings;
        public SnifferSettings.InputSystemType SysType => _snifferSettings.InputSystem;

        public void Init()
        {
            _state = SnifferState.IDLE;
            _snifferSettings = Resources.Load<SnifferSettings>("SnifferSettings");
            InitDependencies();
            InitObserver();
        }

        private void RecorderOnOnReplayFinished()
        {
            _state = SnifferState.IDLE;
        }

        private void InitDependencies()
        {
            _recorder = _snifferSettings.InputSystem == SnifferSettings.InputSystemType.NEW_INPUT
                ? (IRecorder)new NewInpSysRecorder()
                : (IRecorder)new OldInpSysRecorder();
            
            _recorder.OnReplayFinished += RecorderOnOnReplayFinished;
        }
        
        private void InitObserver()
        {
            var lifeCycle = Object.Instantiate(_snifferSettings.SnifferObserver);
            lifeCycle.gameObject.name = "SnifferObserver";
            lifeCycle.Subscribe(_recorder);
        }
        
        public void Stop()
        {
            _state = SnifferState.IDLE;
            _recorder.StopRec();
        }

        public void Record()
        {
            _state = SnifferState.RECORDING;
            _recorder.StartRec();
        }

        public void Load(string recordingPath)
        {
            _recorder.LoadFromFile(recordingPath);
        }

        public void Replay()
        {
            if(_state == SnifferState.RECORDING)
                Stop();
            
            _state = SnifferState.PLAYING_BACK;
            _recorder.Replay();
        }

        public void StopReplay()
        {
            _state = SnifferState.IDLE;
        }

        public void Save(string recordingFileName)
        {
            _recorder.SaveToFile(recordingFileName);
        }

        public void Clear()
        {
            _recorder.Clear();
        }
        
        public void Pause()
        {
            _recorder.Pause();    
        }

        ~SnifferCore()
        {
            if(_recorder != null)
                _recorder.OnReplayFinished += RecorderOnOnReplayFinished;
        }
    }
}
