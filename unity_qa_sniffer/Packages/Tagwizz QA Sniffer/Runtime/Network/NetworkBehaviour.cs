using System;
using System.Collections;
using System.IO;
using System.Net.Sockets;
using TagwizzQASniffer.Core;
using TagwizzQASniffer.Core.FramesRecorder;
using TagwizzQASniffer.Core.Recording;
using TagwizzQASniffer.Exceptions;
using TagwizzQASniffer.Network.Clients;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace TagwizzQASniffer.Network
{
   
    enum CommandSignal { RECORD, STOP_REC, REPLAY, STOP_REPLAY, LOAD_FILE, SAVE_FILE,GET_DEVICE_DATA, CHANGE_STATE }
    public enum NetworkState {CONNECTED, DISCONNECTED}

    public class NetworkBehaviour : MonoBehaviour, IRecorderListener, IFramesRecorderListener, IClientListener
    {
        
        private CanvasGroup _canvasGroup;
        private SnifferCore _snifferCore;
        private HubClient _hubClient;
        private FileClient _fileClient;
        private StreamingClient _streamingClient;
        private readonly string _serverIp = "";
        private NetworkState _state;
        public NetworkState State => _state;

        public HubClient HubClient => _hubClient;
        public StreamingClient StreamingClient => _streamingClient;
        public Action networkInitialized;
        

        private void Awake()
        {
            InitNetworkComponents();
        }

        private void InitNetworkComponents()
        {
            _hubClient = new HubClient();
            _fileClient = new FileClient();
            _streamingClient = new StreamingClient();
            
            _hubClient.Observer.Subscribe(this);
            _streamingClient.Observer.Subscribe(this);
            _state = NetworkState.DISCONNECTED;
            networkInitialized?.Invoke();
        }

        private void Start()
        {
            _hubClient.OnReceivedMsgFromServerEvent += HubClientOnReceivedMsgFromServer;
            _canvasGroup = GetComponentInChildren<CanvasGroup>();
            _state = NetworkState.DISCONNECTED;
        }

        private void HubClientOnReceivedMsgFromServer(string message)
        {
           DecodeMessage(message); 
        }


        private void DecodeMessage(string message)
        {
            var state = _snifferCore.State;
            if (message == CommandSignal.RECORD.ToString() && state == SnifferState.IDLE)
                _snifferCore.Record();
            else if (message == CommandSignal.STOP_REC.ToString() && state == SnifferState.RECORDING)
                _snifferCore.Stop();
            else if (message == CommandSignal.REPLAY.ToString() && state == SnifferState.IDLE)
                _snifferCore.Replay();
            else if (message == CommandSignal.STOP_REPLAY.ToString() && state == SnifferState.PLAYING_BACK)
                _snifferCore.StopReplay();
            else if (message == CommandSignal.SAVE_FILE.ToString() && state == SnifferState.IDLE)
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
            else if (message == CommandSignal.LOAD_FILE.ToString() && state == SnifferState.IDLE)
            {
                try {
                    _fileClient.LoadFile(_serverIp, NetworkDefinitions.FILE_PORT, _snifferCore);
                }
                catch (SnifferCoreLoadFileException e) {
                    _hubClient.SendMsgToServer(e.Message);
                }
                catch (LoadFileNetworkErrorException e) {
                    _hubClient.SendMsgToServer(e.Message);
                }
            }
            else
            {
                Debug.Log($"Error trying to execute command: {message} ");
                SendCommandErrorMsg(state,message);
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
            _snifferCore.FramesRecorder.Observer.Subscribe(this);
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
            
            try {
                _streamingClient.StartClient(serverIp, NetworkDefinitions.STREAMING_PORT);
                _state = NetworkState.CONNECTED;
            }
            catch (NetworkServerConnectionErrorException exp) {
                Debug.Log($"Error trying to connect streaming client: {exp.Message}");
                _state = NetworkState.DISCONNECTED;
            }

            try {
                _hubClient.StartClient(serverIp, port);
                _state = NetworkState.CONNECTED;
            }
            catch (NetworkServerConnectionErrorException exp) {
                Debug.Log($"Error trying to connect hub client: {exp.Message}");
                _state = NetworkState.DISCONNECTED;
            }


            return _state == NetworkState.CONNECTED;
        }

        public void Disconnect()
        {
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
            if (!IsActivated()) return;
            
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

        private bool IsActivated()
        {
            return Input.touchCount >= 4 || Keyboard.current.f1Key.wasPressedThisFrame;
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

        void IClientListener.Connected()
        {
        }

        void IClientListener.Disconnected()
        {
        }

        void IClientListener.ExceptionThrown()
        {
            Debug.Log($"Connection exception occured");
            InitNetworkComponents();
        }
    }
}
