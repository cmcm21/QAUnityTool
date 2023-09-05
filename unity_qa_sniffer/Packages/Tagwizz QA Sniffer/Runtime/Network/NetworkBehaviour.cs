using System.IO;
using TagwizzQASniffer.Core;
using TagwizzQASniffer.Core.FramesRecorder;
using TagwizzQASniffer.Core.Recording;
using TagwizzQASniffer.Exceptions;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

namespace TagwizzQASniffer.Network
{
   
    enum CommandSignal { RECORD, STOP_REC, REPLAY, STOP_REPLAY, LOAD_FILE, SAVE_FILE,GET_DEVICE_DATA, CHANGE_STATE }

    public class NetworkBehaviour : MonoBehaviour, IRecorderListener, IFramesRecorderListener
    {
        [SerializeField] private TMP_InputField ipInput;
        [SerializeField] private TMP_InputField portInput;
        [SerializeField] private TextMeshProUGUI logger;
        
        private CanvasGroup _canvasGroup;
        private string GetIp => ipInput.text;
        private int GetPort => int.Parse(portInput.text);
        
        private SnifferCore _snifferCore;
        private HubClient _client;
        private FileClient _fileClient;
        private StreamingClient _streamingClient;
        private void Start()
        {
            _client = new HubClient();
            _client.OnReceivedMsgFromServerEvent += ClientOnReceivedMsgFromServer;
            _fileClient = new FileClient();
            _streamingClient = new StreamingClient();
             
            _canvasGroup = GetComponentInChildren<CanvasGroup>();
        }

        private void ClientOnReceivedMsgFromServer(string message)
        {
           DecodeMessage(message); 
        }

        private void ClientOnServerConnectionMadeEvent(string server)
        {
            logger.SetText($"Connected to {server}");
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
                    _fileClient.SaveFile(GetIp, 44444, _snifferCore);
                }
                catch (SnifferCoreSavingFileError e)
                {
                    _client.SendMsgToServer(e.Message);
                }
                catch (SaveFileNetworkErrorException e)
                {
                   _client.SendMsgToServer(e.Message); 
                }
            }
            else if (message == CommandSignal.LOAD_FILE.ToString() && state == SnifferState.IDLE)
            {
                try {
                    _fileClient.LoadFile(GetIp, 44444, _snifferCore);
                }
                catch (SnifferCoreLoadingFileError e) {
                    _client.SendMsgToServer(e.Message);
                }
                catch (LoadFileNetworkErrorException e) {
                    _client.SendMsgToServer(e.Message);
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
            _client.SendMsgToServer($"Sniffer cannot perform a {action} action. Sniffer core state: {state}");
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
            _client.SendMsgToServer($"{_snifferCore.State}");
        }
        

        private void Show()
        {
            _canvasGroup.alpha = 1;
        }

        private void Hide()
        {
            _canvasGroup.alpha = 0;
        } 

        public void Connect()
        {
            if (GetIp == string.Empty || portInput.text == string.Empty || _canvasGroup.alpha == 0) return;
            if (!ValidateInputs()) return;
            if (_client.isReading) return;
            
            _client.StartClient(GetIp,GetPort);
            _streamingClient.StartClient(GetIp);
            portInput.interactable = false;
            ipInput.interactable = false;
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
            return Input.touches.Length >= 4 || Keyboard.current.f1Key.wasPressedThisFrame;
        }

        private void OnDestroy()
        {
            _client.OnReceivedMsgFromServerEvent -= ClientOnReceivedMsgFromServer;
            _client.StopClient();
            _fileClient.StopClient();

            if (_snifferCore != null)
            {
                _snifferCore.Recorder?.Unsubscribe(this);
                _snifferCore?.Destroy();
            }
        }

        private void OnApplicationQuit()
        {
            _client.StopClient();
            _fileClient.StopClient();
            _streamingClient.StopSocket();
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
   }
}
