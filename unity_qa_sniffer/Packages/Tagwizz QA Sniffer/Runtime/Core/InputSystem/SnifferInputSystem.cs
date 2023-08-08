using System;
using System.Collections.Generic;
using Unity.Plastic.Newtonsoft.Json.Serialization;
using Action = System.Action;

namespace TagwizzQASniffer.Core.InputSystem
{
    public interface ISnifferInputSystem
    {
        public event Action<InputData> InputStarted;
        public event Action<InputData> InputEnded;
        public void Init();
        public void ReadInputs();
        public void Stop();
        public List<InputData> GetInputData();
    }
    
    [System.Serializable]
    public enum InputType{TOUCH,KEY,BUTTON}
}