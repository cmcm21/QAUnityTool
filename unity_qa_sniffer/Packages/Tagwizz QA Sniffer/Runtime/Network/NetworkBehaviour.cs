using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TagwizzQASniffer.Core;
using TagwizzQASniffer.Core.FramesRecorder;
using TagwizzQASniffer.Core.Recording;
using TagwizzQASniffer.Exceptions;
using TagwizzQASniffer.Network.Clients;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TagwizzQASniffer.Network
{
   
    enum CommandSignal { RECORD, STOP_REC, REPLAY, STOP_REPLAY, LOAD_FILE, SAVE_FILE, SET_HOSTNAME, REPLAY_ONE_STEP}
    public enum NetworkState {CONNECTED, DISCONNECTED}

    public class NetworkBehaviour : MonoBehaviour, IRecorderListener, IFramesRecorderListener, IClientListener
    {
        
        private CanvasGroup _canvasGroup;
        private SnifferCore _snifferCore;
        private HubClient _hubClient;
        private FileClient _fileClient;
        private StreamingClient _streamingClient;
        private string _serverIp = "";
        private NetworkState _state;
        private string _deviceName;
        public NetworkState State => _state;

        public HubClient HubClient => _hubClient;
        public StreamingClient StreamingClient => _streamingClient;
        public Action networkInitialized;
        // we use a queue in order to execute commands in the main thread of the game (needed for inputRecorder)
        private Queue<string> _commandsFromServer; 

        private void Awake()
        {
            _deviceName = SystemInfo.deviceName;
            _commandsFromServer = new Queue<string>(); 
            InitNetworkComponents();
        }

        private void InitNetworkComponents()
        {
            _hubClient = new HubClient();
            _fileClient = new FileClient();
            _streamingClient = new StreamingClient();
            _hubClient.OnReceivedMsgFromServerEvent += HubClientOnReceivedMsgFromServer;
            
            _hubClient.Observer.Subscribe(this);
           // _streamingClient.Observer.Subscribe(this);
            _state = NetworkState.DISCONNECTED;
            networkInitialized?.Invoke();
        }

        private void Start()
        {
            _canvasGroup = GetComponentInChildren<CanvasGroup>();
            _state = NetworkState.DISCONNECTED;
            
            Debug.Log($"System Information: {SystemInfo.deviceName}");
            Init();
            Show();
        }

        private void HubClientOnReceivedMsgFromServer(string message)
        {
            _commandsFromServer.Enqueue(message);
        }


        private void DecodeMessage(string message)
        {
            var state = _snifferCore.State;
            if (message == CommandSignal.RECORD.ToString() && state == SnifferState.IDLE)
                _snifferCore.Record();
            else if (message == CommandSignal.STOP_REC.ToString() && state == SnifferState.RECORDING)
                _snifferCore.Stop();
            else if (message == CommandSignal.REPLAY.ToString() 
                     && (state == SnifferState.IDLE || state == SnifferState.PLAYING_STEPBYSTEP))
                _snifferCore.Replay();
            else if(message == CommandSignal.REPLAY_ONE_STEP.ToString() 
                     && (state == SnifferState.IDLE || state == SnifferState.PLAYING_STEPBYSTEP))
                _snifferCore.ReplayOneStep();
            else if (message == CommandSignal.STOP_REPLAY.ToString() 
                     && (state == SnifferState.PLAYING_BACK || state == SnifferState.PLAYING_STEPBYSTEP))
                _snifferCore.StopReplay();
            else if (message == CommandSignal.SAVE_FILE.ToString() && state == SnifferState.IDLE)
                ProcessSaveCommand();
            else if (message == CommandSignal.LOAD_FILE.ToString() && state == SnifferState.IDLE)
                ProcessLoadCommand();
            else
            {
                Debug.Log($"Error trying to execute command: {message} ");
                SendCommandErrorMsg(state,message);
            }
        }

        private void ProcessSaveCommand()
        {
             try
             {
                 _fileClient.SaveFile(_serverIp, NetworkDefinitions.FILE_PORT, _snifferCore);
             }
             catch (SnifferCoreSaveFileException e)
             {
                 _hubClient.SendMsgToServer(e.Message);
             }
             catch (SaveFileNetworkErrorException e)
             {
                 _hubClient.SendMsgToServer(e.Message); 
             }
        }

        private void ProcessLoadCommand()
        {
            try 
            {
                _fileClient.LoadFile(_serverIp, NetworkDefinitions.FILE_PORT, _snifferCore);
            }
            catch (SnifferCoreLoadFileException e) {
                _hubClient.SendMsgToServer(e.Message);
            }
            catch (LoadFileNetworkErrorException e) {
                _hubClient.SendMsgToServer(e.Message);
            }
        }

        private void SendCommandErrorMsg(SnifferState state, string action)
        {
            _hubClient.SendMsgToServer($"Sniffer cannot perform a {action} action. Sniffer core state: {state}");
        }

        private void Init()
        {
            _snifferCore = new SnifferCore();
            _snifferCore.Init();
            ConnectEvents();
            Show();
        }

        private void ConnectEvents()
        {
            _snifferCore.Recorder.Subscribe(this);
            _snifferCore.framesCapture.Observer.Subscribe(this);
        }

        private void SendServerSnifferCodeChangedState()
        {
            _hubClient.SendMsgToServer($"{_snifferCore.State}");
        }
        

        private void Show()
        {
            _canvasGroup.alpha = 1;
        }

        private void Hide()
        {
            _canvasGroup.alpha = 0;
        } 

        public bool Connect(string serverIp, int port)
        {
            
            if (serverIp == string.Empty || port == 0 || _canvasGroup.alpha == 0) return false;
            if (!ValidateInputs()) return false;
            if (_hubClient.isReading) return false;
            _serverIp = serverIp;
 
            try
            {
                _hubClient.StartClient(serverIp, port);
                _state = NetworkState.CONNECTED;
            }
            catch (NetworkServerConnectionErrorException exp)
            {
                Debug.Log($"Error trying to connect hub client: {exp.Message}");
                _state = NetworkState.DISCONNECTED;
            }

            if (_snifferCore.SnifferSettings.LiveStreaming)
            {
                try {
                    _streamingClient.StartClient(serverIp, NetworkDefinitions.STREAMING_PORT);
                    _state = NetworkState.CONNECTED;
                }
                catch (NetworkServerConnectionErrorException exp) {
                    Debug.Log($"Error trying to connect streaming client: {exp.Message}");
                    _state = NetworkState.DISCONNECTED;
                }

            }

            return _state == NetworkState.CONNECTED;
        }
        

        public void Disconnect()
        {
            _state = NetworkState.DISCONNECTED;
            if(_snifferCore.State == SnifferState.RECORDING)
                _snifferCore.Stop();
            
            _hubClient.StopClient(); 
            _streamingClient.StopClient();
            _fileClient.StopClient();
        }
        
        private bool ValidateInputs()
        {
            return true;
        }
        
        private void Update()
        {
            if (_commandsFromServer.Count > 0)
                DecodeMessage(_commandsFromServer.Dequeue());

            if (IsActivated())
            {
                if(_snifferCore == null)
                    Init();
                else
                {
                    if (_canvasGroup.alpha == 0)
                        Show();
                    else
                        Hide();
                }
            }
        }

        private bool IsActivated()
        {
            if (Keyboard.current == null)
                return false;
            return Keyboard.current.f1Key.wasPressedThisFrame;
        }

        private void OnDestroy()
        {
            _hubClient.OnReceivedMsgFromServerEvent -= HubClientOnReceivedMsgFromServer;
            _hubClient.StopClient();
            _fileClient.StopClient();

            if (_snifferCore != null)
            {
                _snifferCore.Recorder?.Unsubscribe(this);
                _snifferCore?.Destroy();
            }
        }

        private void OnApplicationQuit()
        {
            _hubClient.StopClient();
            _fileClient.StopClient();
            _streamingClient.StopClient();
            _snifferCore?.Stop();
        }

        private void OnConnectionInterrupted()
        {
            if (_snifferCore == null) return;
            
            if(_snifferCore.State == SnifferState.RECORDING)
                _snifferCore.Stop();
            else if(_snifferCore.State == SnifferState.PLAYING_BACK)
                _snifferCore.StopReplay();
        }
        
        

        #region IRecorderListener 

        void IRecorderListener.OnRecordStarted()
        {
            SendServerSnifferCodeChangedState();
        }
 
        void IRecorderListener.OnRecordFinished() 
        {
            SendServerSnifferCodeChangedState();
        }
 
        void IRecorderListener.OnReplayStarted() 
        {
            SendServerSnifferCodeChangedState();
        }
 
        void IRecorderListener.OnReplayFinished() 
        {
            SendServerSnifferCodeChangedState();
        }

        void  IRecorderListener.OnReplayStepByStepStarted()
        {
            SendServerSnifferCodeChangedState();
        }

        #endregion

        #region IFramesRecorderListener 
        void IFramesRecorderListener.FrameRecorded(MemoryStream stream)
        {
            _streamingClient.AddFrameToSend(stream);
        }
 
        void IFramesRecorderListener.Started()
        {
        }
 
        void IFramesRecorderListener.Stopped()
        {
        }
        #endregion

        #region ClientListener 

        void IClientListener.Connected()
        {
            if(_hubClient.IsConnected)
                _hubClient.SendMsgToServer($"{CommandSignal.SET_HOSTNAME}:{_deviceName}");
        }
 
        void IClientListener.Disconnected()
        {
            OnConnectionInterrupted();
            _state = NetworkState.DISCONNECTED;
        }
 
        #endregion
    }
}
