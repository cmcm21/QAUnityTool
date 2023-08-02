using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace Tagwizz_QA_Sniffer.Core.InputSystem.OldSystemInput
{
    public class OldSystemInput: SnifferInputSystem
    {
        private readonly List<InputTracker> _trackers = new List<InputTracker>();
        private MouseInputTracker _mouseInputTracker;
        private KeyInputTracker _keyInputTracker;
        private AxesInputTracker _axesInputTracker;
        private TouchInputTracker _touchInputTracker;
        
        public override void Init()
        {
            _mouseInputTracker = new MouseInputTracker();
            _keyInputTracker = new KeyInputTracker();
            _axesInputTracker = new AxesInputTracker();
            _touchInputTracker = new TouchInputTracker();
            
            _trackers.Add(_mouseInputTracker);
            _trackers.Add(_keyInputTracker);
            _trackers.Add(_axesInputTracker);
            _trackers.Add(_touchInputTracker);
        }

        public override async void GetInputs()
        {
            foreach (var tracker in _trackers)
                tracker.CheckInputs();
        }
    }
}