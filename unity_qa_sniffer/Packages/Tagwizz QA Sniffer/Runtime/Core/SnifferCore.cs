using TagwizzQASniffer.Core.InputSystem;
using TagwizzQASniffer.Core.InputSystem.OldSystemInput;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TagwizzQASniffer.Core 
{
    public enum SnifferState {RECORDING,IDLE,PLAYING_BACK}
    public class SnifferCore: ILifeCycleSubscriber 
    {
        private SnifferState _state;
        public SnifferState State => _state;

        private SnifferInputSystem _inputSystem;

        public void Init()
        {
            Debug.Log($"[{GetType()}]Sniffer Init");
            _state = SnifferState.IDLE;
            InitLifeCycle(); 
            SetInputSystem();
        }

        private void SetInputSystem()
        {
            _inputSystem = new OldSystemInput();
            _inputSystem.Init();
        }

        private void InitLifeCycle()
        {
            //TODO: GET PATH FROM SNIFFER EDITOR or vice-versa
            var snifferSettings = Resources.Load<SnifferSettings>("SnifferSettings");
            var lifeCycle = Object.Instantiate(snifferSettings.LifeCycle);
            lifeCycle.gameObject.name = "LifeCycle";
            lifeCycle.Subscribe(this);
        }
        
        public void Stop()
        {
            _state = SnifferState.IDLE;
            _inputSystem.Stop();
            //get data from input system and store it in a json file
        }

        public void Recording()
        {
            _state = SnifferState.RECORDING;
        }

        public void OnAwake() { }

        public void OnStart() { }

        public void OnUpdate()
        {
            switch (_state)
            {
               case SnifferState.RECORDING: _inputSystem.GetInputs(); break;
               case SnifferState.PLAYING_BACK: break;
            }
        }
    }
}
