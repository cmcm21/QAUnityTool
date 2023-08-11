using System;
using System.Collections.Generic;

namespace TagwizzQASniffer.Core.InputSystem.OldSystemInput
{
    public enum InputType {KEY,TOUCH,BUTTON}
    public class OldInputSystem
    {
        private readonly List<InputTracker> _trackers = new List<InputTracker>();
        private readonly List<InputData> _inputData = new List<InputData>();
        private MouseInputTracker _mouseInputTracker;
        private KeyInputTracker _keyInputTracker;
        private AxesInputTracker _axesInputTracker;
        private TouchInputTracker _touchInputTracker;

        public event Action<InputData> InputStarted;
        public event Action<InputData> InputEnded;

        public void Init()
        {
            AddKeyboardTracker();
            AddAxesTracker();
            foreach (var track in _trackers)
            {
                 track.TrackEnded += OnTrackEnded;
                 track.TrackStarted += OnTrackStarted;
            }
        }

        private void AddKeyboardTracker()
        {
            _keyInputTracker = new KeyInputTracker();
            _trackers.Add(_keyInputTracker);
        }

        private void AddAxesTracker()
        {
            _axesInputTracker = new AxesInputTracker();
            _trackers.Add(_axesInputTracker);
        }

        private void AddMouseTracker()
        {
            _mouseInputTracker = new MouseInputTracker();
            _trackers.Add(_mouseInputTracker);
        }

        private void AddTouchTracker()
        {
            _touchInputTracker = new TouchInputTracker();
            _trackers.Add(_touchInputTracker);
        }

        private void OnTrackStarted(InputData inputData)
        {
            InputStarted?.Invoke(inputData);
        }

        private void OnTrackEnded(InputData inputData)
        {
            InputEnded?.Invoke(inputData);
            _inputData.Add(inputData);
        }

        public void ReadInputs()
        {
            foreach (var tracker in _trackers)
                tracker.CheckInputs();
        }

        public void Stop()
        {
            foreach(var tracker in _trackers)
                tracker.StopTracker();
        }

        public List<InputData> GetInputData()
        {
            return _inputData;
        }
    }
}