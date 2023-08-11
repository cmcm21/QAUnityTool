using UnityEngine;
using UnityEngine.InputSystem;

namespace TagwizzQASniffer.Core.Recording
{
    public class NewInpSysRecorder : IRecorder
    {
        private InputRecorder _inputRecorder;

        public NewInpSysRecorder()
        {
            _inputRecorder = new InputRecorder();
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

        public void StartRec()
        {
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

        public void Play()
        {
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
    }
}
