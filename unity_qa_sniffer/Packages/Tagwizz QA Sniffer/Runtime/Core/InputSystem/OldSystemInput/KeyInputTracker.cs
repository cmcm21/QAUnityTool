using System.Collections.Generic;
using Codice.Client.Common.WebApi;
using UnityEngine;

namespace TagwizzQASniffer.Core.InputSystem.OldSystemInput
{
    public class KeyInputTracker: InputTracker
    {
        private Dictionary<string, List<InputData>> _keysRef = new Dictionary<string, List<InputData>>();
        public KeyInputTracker()
        {
        }
        public override async void CheckInputs() { }
    }
}