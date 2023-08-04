using TagwizzQASniffer.Core.InputSystem.OldSystemInput;
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

        public Recorder _recorder;

        public void Init()
        {
            _state = SnifferState.IDLE;
            InitDependencies();
            InitLifeCycle();
        }

        private void InitDependencies()
        {
            _recorder = new Recorder(typeof(OldSystemInput));
        }
        
        private void InitLifeCycle()
        {
            var snifferSettings = Resources.Load<SnifferSettings>("SnifferSettings");
            var lifeCycle = Object.Instantiate(snifferSettings.LifeCycle);
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
    }
}
