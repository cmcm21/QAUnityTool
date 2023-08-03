using System;
using System.Text;
using TagwizzQASniffer.Core.InputSystem;
using TagwizzQASniffer;
using TagwizzQASniffer.Core.InputSystem.OldSystemInput;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TagwizzQASniffer.Core 
{
    public enum SnifferState {RECORDING,PLAYING_BACK}
    public class SnifferCore 
    {
        private SnifferState _state;
        public SnifferState State => _state;

        private SnifferInputSystem _inputSystem;

        public void Init()
        {
            Debug.Log($"[{GetType()}]Sniffer Init");
            InitLifeCycle(); 
            //TODO: Select input system depending on the package that is installed in the project
            _inputSystem = new OldSystemInput();
            _inputSystem.Init();
        }

        private void InitLifeCycle()
        {
            //TODO: GET PATH FROM SNIFFER EDITOR or vice-versa
            var snifferSettings = Resources.Load<SnifferSettings>("SnifferSettings");
            var lifeCycle = GameObject.Instantiate(snifferSettings.LifeCycle);
        }
        

        public void Update()
        {
            _inputSystem.GetInputs();
        }

        public void Stop()
        {
            _inputSystem.Stop();
        }
    }
}
