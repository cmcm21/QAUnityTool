using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace TagwizzQASniffer.Core.InputSystem.NewSystemInput
{
    public class NewInputSystem : ISnifferInputSystem
    {
        public event Action<InputData> InputStarted;
        public event Action<InputData> InputEnded;
        
        public void Init()
        {
        }

        public void ReadInputs()
        {
        }

        public void Stop()
        {
        }

        public List<InputData> GetInputData()
        {
            return new List<InputData>();
        }
    }
}
