using System.IO;
using TagwizzQASniffer.Core.Recording;
using TagwizzQASniffer.Core.FramesRecorder;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TagwizzQASniffer.Core 
{
    public enum SnifferState {RECORDING,IDLE,PLAYING_BACK}
    public class SnifferCore : IRecorderListener, IFramesRecorderListener
    {
        private SnifferState _state;
        public SnifferState State => _state;

        private IRecorder _recorder;
        public IRecorder Recorder => _recorder;
        private FrameRecorder _framesRecorder;
        public FrameRecorder FramesRecorder => _framesRecorder;
        private SnifferSettings _snifferSettings;
        public SnifferSettings.InputSystemType SysType => _snifferSettings.InputSystem;

        public void Init()
        {
            _state = SnifferState.IDLE;
            _snifferSettings = Resources.Load<SnifferSettings>("SnifferSettings");
            InitDependencies();
            InitObserver();
            
            if(_snifferSettings.RecordFrames)
                InitFramesRecorder();
        }

        public void Destroy()
        {
            if(_recorder != null)
                _recorder.Unsubscribe(this);
        }

        private void InitDependencies()
        {
            _recorder = _snifferSettings.InputSystem == SnifferSettings.InputSystemType.NEW_INPUT
                ? (IRecorder)new NewInpSysRecorder()
                : (IRecorder)new OldInpSysRecorder();

            _recorder.Subscribe(this);
        }
        
        private void InitObserver()
        {
            var lifeCycle = Object.Instantiate(_snifferSettings.SnifferObserver);
            lifeCycle.gameObject.name = "SnifferObserver";
            lifeCycle.Subscribe(_recorder);
        }

        private void InitFramesRecorder()
        {
            var currentCamera = Camera.current;
            if (currentCamera == null)
            {
                var cameras = Object.FindObjectsOfType<Camera>();
                foreach (var camera in cameras)
                {
                    if (camera.isActiveAndEnabled)
                        currentCamera = camera;
                }
            }
            _framesRecorder = currentCamera.gameObject.AddComponent<FrameRecorder>();
            _framesRecorder.frameRate = _snifferSettings.FrameRate;
            _framesRecorder.maxFrames = _snifferSettings.MaxFramesPerRec;
            _framesRecorder.Observer.Subscribe(this);
        }
        
        public void Stop()
        {
            _state = SnifferState.IDLE;
            _recorder.StopRec();
            
            if(_framesRecorder != null) _framesRecorder.StopRecording();
        }

        public void Record()
        {
            _state = SnifferState.RECORDING;

            _recorder.StartRec();
            PlayFrameRecorder();
        }

        private void PlayFrameRecorder()
        {
             if (_framesRecorder != null)
             {
                 if(_framesRecorder.State == FrameRecorderState.RECORDING)
                     _framesRecorder.StopRecording();
                 _framesRecorder.StartRecording();
             }
        }

        public void Load(string recordingPath)
        {
            _recorder.LoadFromFile(recordingPath);
        }

        public void Replay()
        {
            if(_state == SnifferState.RECORDING)
                Stop();
            
            _state = SnifferState.PLAYING_BACK;
            PlayFrameRecorder();
            _recorder.Replay();
        }

        public void StopReplay()
        {
            _state = SnifferState.IDLE;
            _recorder.StopPlay();
            
            if(_framesRecorder != null) _framesRecorder.StopRecording();
        }

        public void Save(string recordingFileName)
        {
            _recorder.SaveToFile(recordingFileName);
        }

        public void Clear()
        {
            _recorder.Clear();
        }
        
        public void Pause()
        {
            _recorder.Pause();    
        }

        #region IRecorderListener 

        void IRecorderListener.OnRecordStarted()
        {
            _state = SnifferState.RECORDING;
        }
 
        void IRecorderListener.OnRecordFinished()
        {
            _state = SnifferState.IDLE;
        }
 
        void IRecorderListener.OnReplayStarted()
        {
            _state = SnifferState.PLAYING_BACK;
        }
 
        void IRecorderListener.OnReplayFinished() 
        {
            _state = SnifferState.IDLE;
            if(_framesRecorder != null && _framesRecorder.State == FrameRecorderState.RECORDING)
                _framesRecorder.StopRecording();
        }
 

        #endregion
       #region IFrameRecorderListener 
        void IFramesRecorderListener.FrameRecorded(MemoryStream stream)
        {
        }
 
        void IFramesRecorderListener.Started()
        {
            Debug.Log("Frames recorder started");
        }
 
        void IFramesRecorderListener.Stopped()
        {
            Debug.Log("Frames recorder stopped");
        }       
        #endregion

    }
}
