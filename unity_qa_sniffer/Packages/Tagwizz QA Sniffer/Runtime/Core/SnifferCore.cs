using TagwizzQASniffer.Core.InputSystem.OldSystemInput;
using TagwizzQASniffer.Core.InputSystem.NewSystemInput;
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

        private Recorder _recorder;
        private SnifferSettings _snifferSettings;

        public void Init()
        {
            _state = SnifferState.IDLE;
            _snifferSettings = Resources.Load<SnifferSettings>("SnifferSettings");
            InitDependencies();
            InitLifeCycle();
        }

        private void InitDependencies()
        {
            var inputType = _snifferSettings.InputSystem == SnifferSettings.InputSystemType.NEW_INPUT
                ? typeof(NewInputSystem)
                : typeof(OldInputSystem);
            
            _recorder = new Recorder(inputType);
        }
        
        private void InitLifeCycle()
        {
            var lifeCycle = Object.Instantiate(_snifferSettings.LifeCycle);
            lifeCycle.gameObject.name = "LifeCycle";
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

        public void SaveRecord(string recordingFileName)
        {
            if (_state == SnifferState.IDLE)
            {
                var recData = _recorder.GetRecordingData();
                RecordingFileManager.SaveToJson(recData,recordingFileName); 
            }
        }
    }
}
