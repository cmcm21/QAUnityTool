using System.IO;
using TagwizzQASniffer.Core.Recording;
using TagwizzQASniffer.Core.FramesRecorder;
using TagwizzQASniffer.Exceptions;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TagwizzQASniffer.Core 
{
    public enum SnifferState {RECORDING,IDLE,PLAYING_BACK, PLAYING_STEPBYSTEP}
    public class SnifferCore : IRecorderListener, IFramesRecorderListener
    {
        private SnifferState _state;
        public SnifferState State => _state;

        private IRecorder _recorder;
        public IRecorder Recorder => _recorder;
        private FrameCapture _framesCapture;
        public FrameCapture framesCapture => _framesCapture;
        private SnifferSettings _snifferSettings;
        public SnifferSettings.InputSystemType SysType => _snifferSettings.InputSystem;
        public SnifferSettings SnifferSettings => _snifferSettings;

        public void Init()
        {
            _state = SnifferState.IDLE;
            _snifferSettings = Resources.Load<SnifferSettings>("SnifferSettings");
            InitDependencies();
            InitObserver();
            
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
                ? (IRecorder)new NewInpSysRecorder(_snifferSettings)
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
            _framesCapture = currentCamera.gameObject.AddComponent<FrameCapture>();
            _framesCapture.frameRate = _snifferSettings.FrameRate;
            _framesCapture.maxFrames = _snifferSettings.MaxFramesPerRec;
            _framesCapture.Observer.Subscribe(this);
        }
        
        public void Stop()
        {
            _state = SnifferState.IDLE;
            _recorder.StopRec();
            
            if(_framesCapture != null) _framesCapture.StopRecording();
            
            Debug.Log("Sniffer Core Stopped");
        }

        public void Record()
        {
            _state = SnifferState.RECORDING;

            _recorder.StartRec();
            PlayFrameRecorder();
        }

        private void PlayFrameRecorder()
        {
             if (_framesCapture != null)
             {
                 if(_framesCapture.State == FrameRecorderState.RECORDING)
                     _framesCapture.StopRecording();
                 _framesCapture.StartRecording();
             }
        }

        public void Load(string recordingPath)
        {
            try
            {
                _recorder.LoadFromFile(recordingPath);
            }
            catch (UnityException e)
            {
                Debug.Log($"Exception: {e.Message}, File : {recordingPath} corrupted, it couldn't be loaded");
                throw new SnifferCoreLoadFileException(recordingPath);
            }
        }

        public void Replay()
        {
            if(_state == SnifferState.RECORDING)
                Stop();
            
            _state = SnifferState.PLAYING_BACK;
            PlayFrameRecorder();
            _recorder.Replay();
            Debug.Log("Sniffer Core Stopped Replay");
        }

        public void StopReplay()
        {
            _state = SnifferState.IDLE;
            _recorder.StopPlay();
            
            if(_framesCapture != null) _framesCapture.StopRecording();
        }

        public void ReplayOneStep()
        {
            if(_state == SnifferState.RECORDING)
                Stop();
            
            if(_state == SnifferState.IDLE)
                PlayFrameRecorder();
            
            _state = SnifferState.PLAYING_STEPBYSTEP;
            _recorder.ReplayOneStep();
        }

        public void Save(string recordingFileName)
        {
            try
            {
                _recorder.SaveToFile(recordingFileName);
            }
            catch (UnityException e)
            {
               Debug.Log($"Exception: {e.Message}, File corrupted: {recordingFileName}, it couldn't be saved");
               throw new SnifferCoreSaveFileException(recordingFileName);
            }
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
            if(_framesCapture != null && _framesCapture.State == FrameRecorderState.RECORDING)
                _framesCapture.StopRecording();
        }

        void IRecorderListener.OnReplayStepByStepStarted()
        {
            _state = SnifferState.PLAYING_STEPBYSTEP;
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
