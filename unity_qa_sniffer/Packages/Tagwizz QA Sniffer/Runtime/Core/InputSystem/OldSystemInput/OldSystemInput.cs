using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TagwizzQASniffer.Core.InputSystem.OldSystemInput
{
    public class OldSystemInput: ISnifferInputSystem
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
            _mouseInputTracker = new MouseInputTracker();
            _keyInputTracker = new KeyInputTracker();
            _axesInputTracker = new AxesInputTracker();
            _touchInputTracker = new TouchInputTracker();
            
            _trackers.Add(_mouseInputTracker);
            _trackers.Add(_keyInputTracker);
            _trackers.Add(_axesInputTracker);
            _trackers.Add(_touchInputTracker);

            foreach (var track in _trackers)
            {
                 track.TrackEnded += OnTrackEnded;
                 track.TrackStarted += OnTrackStarted;
            }
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

        public List<InputData> GetInputData()
        {
            return _inputData;
        }
    }
}