using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TagwizzQASniffer.Core.Recording
{
    public class NewInpSysRecorder : IRecorder
    {
        private readonly InputRecorder _inputRecorder;

        private readonly List<IRecorderListener> _recorderListeners = new List<IRecorderListener>();
        
        public NewInpSysRecorder(SnifferSettings settings)
        {
            _inputRecorder = new InputRecorder
            {
                simulateOriginalTimingOnReplay = settings.SimulateOriginalTimingOnReplay,
                recordFrames = settings.RecordInputFrames,
                replayOnNewDevices = settings.ReplayOnNewDevices,
                recordStateEventsOnly = settings.RecordStateEventOnly
            };
            
            _inputRecorder.changeEvent.AddListener((state, inputEventPtr) => {
                switch (state) {
                    case InputRecorder.Change.ReplayStopped:
                        Debug.Log("Replay stopped");
                        _recorderListeners.ForEach(l => l.OnReplayFinished());
                        break;
                    case InputRecorder.Change.ReplayStarted:
                        _recorderListeners.ForEach(l => l.OnReplayStarted());
                        break;
                    case InputRecorder.Change.CaptureStarted:
                        _recorderListeners.ForEach(l => l.OnRecordStarted());
                        break;
                    case InputRecorder.Change.CaptureStopped:
                        _recorderListeners.ForEach(l => l.OnRecordFinished());
                        break;
                    case InputRecorder.Change.ReplayStepByStep:
                        _recorderListeners.ForEach(l=>l.OnReplayStepByStepStarted());
                        break;
                    case InputRecorder.Change.None:
                        break;
                    case InputRecorder.Change.EventCaptured:
                        if(inputEventPtr != null && inputEventPtr.valid)
                            Debug.Log($"[{GetType()}]::Event Capture: {inputEventPtr}");
                        break;
                    case InputRecorder.Change.EventPlayed:
                        if(inputEventPtr != null && inputEventPtr.valid)
                            Debug.Log($"[{GetType()}]::Event Played: {inputEventPtr}");
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(state), state, null);
                }
            });
        }

        public bool Subscribe(IRecorderListener listener)
        {
            if (_recorderListeners.Contains(listener)) 
            {
                return false;
            }
            _recorderListeners.Add(listener);
            return true;
        }

        public void Unsubscribe(IRecorderListener listener)
        {
            _recorderListeners.Remove(listener);
        }

        public void OnAwake() { }

        public void OnStart() { }

        public void OnUpdate() { }

        public void OnEnabled()
        {
            _inputRecorder.OnEnable();
        }

        public void OnDisabled()
        {
            _inputRecorder.OnDisable();
        }

        public void OnDestroy()
        {
            _inputRecorder.OnDestroy();
        }

        public int GetReplayPosition() => _inputRecorder.replayPosition;

        public int GetRecordingLength() =>(int)_inputRecorder.eventCount;
        
        public void StartRec()
        {
            if (!_inputRecorder.captureIsRunning)
            {
                _inputRecorder.ClearCapture();
                _inputRecorder.StartCapture();
            }
        }

        public void StopRec()
        {
            _inputRecorder.StopCapture();
        }

        public void SaveToFile(string fileName)
        {
            _inputRecorder.SaveCaptureToFile(fileName);
        }

        public void Replay()
        {
            if(!_inputRecorder.replayIsRunning)
                _inputRecorder.StartReplay();
        }

        public void StopPlay()
        {
            _inputRecorder.StopReplay();
        }

        public void ReplayOneStep()
        {
           _inputRecorder.ReplayNextEvent(); 
        }

        public void LoadFromFile(string fileName)
        {
            _inputRecorder.LoadCaptureFromFile(fileName);
        }

        public void Clear()
        {
            _inputRecorder.ClearCapture();
        }

        public void Pause() 
        {
            _inputRecorder.PauseReplay();
        }
    }
}
