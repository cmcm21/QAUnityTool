using System;
using TagwizzQASniffer.Core;
using TagwizzQASniffer.Network;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace TagwizzQASniffer.Network
{
   
    enum CommandSignal { RECORD, STOP_REC, REPLAY, STOP_REPLAY, LOAD_FILE, SAVE_FILE,GET_DEVICE_DATA, CHANGE_STATE }

    public class NetworkBehaviour : MonoBehaviour
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
        private void Start()
        {
            _client = new HubClient();
            _client.OnReceivedMsgFromServerEvent += ClientOnReceivedMsgFromServer;
            _fileClient = new FileClient();
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
                _fileClient.SaveFile(GetIp,9999,_snifferCore);
            else if(message == CommandSignal.LOAD_FILE.ToString() && state == SnifferState.IDLE)
                _fileClient.LoadFile(GetIp,9999,_snifferCore);
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
            ConnectRecorderEvents();
            Show();
        }

        private void ConnectRecorderEvents()
        {
            _snifferCore.Recorder.OnReplayStarted += SendServerSnifferCodeChangedState; 
            _snifferCore.Recorder.OnReplayFinished += SendServerSnifferCodeChangedState;
            _snifferCore.Recorder.OnRecordStarted += SendServerSnifferCodeChangedState;
            _snifferCore.Recorder.OnRecordFinished += SendServerSnifferCodeChangedState;
        }

        private void SendServerSnifferCodeChangedState()
        {
            _client.SendMsgToServer($"Sniffer core changed state. Current state: {_snifferCore.State}");
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
        }

        private void OnApplicationQuit()
        {
            _client.StopClient();
            _fileClient.StopClient();
        }
    }
}
