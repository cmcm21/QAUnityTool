using System;
using UnityEditor;
using UnityEngine.InputSystem;

namespace TagwizzQASniffer.Core.Recording
{
    public class NewInpSysRecorder : IRecorder
    {
        private readonly InputRecorder _inputRecorder;
        public event Action OnReplayFinished;
        
        public NewInpSysRecorder()
        {
            _inputRecorder = new InputRecorder
            {
                simulateOriginalTimingOnReplay = true,
                recordFrames = true,
                devicePath = string.Empty
            };
            
            _inputRecorder.changeEvent.AddListener((state) =>
            {
                  if(state == InputRecorder.Change.ReplayStopped)  
                      OnReplayFinished?.Invoke();
            });
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

        public int GetRecLenght() => _inputRecorder.replayPosition;

        public int GetRecPosition() =>(int)_inputRecorder.eventCount;
        
        public void StartRec()
        {
            if(!_inputRecorder.captureIsRunning)
                _inputRecorder.StartCapture();
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
