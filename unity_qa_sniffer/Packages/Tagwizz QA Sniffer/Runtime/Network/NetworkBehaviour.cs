using System;
using TagwizzQASniffer.Core;
using TagwizzQASniffer.Network;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace TagwizzQASniffer.Network
{
   
    enum CommandSignal { RECORD, STOP_REC, REPLAY, STOP_REPLAY, SEND_FILE, GET_FILE,GET_DEVICE_DATA, CHANGE_STATE }

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
        private void Start()
        {
            _client = new HubClient();
            _client.OnReceivedMsgFromServerEvent += ClientOnReceivedMsgFromServer;
            _client.OnServerConnectionMadeEvent += ClientOnServerConnectionMadeEvent;
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
            if (message == CommandSignal.RECORD.ToString())
            {
                if(state == SnifferState.IDLE)
                    _snifferCore.Record();
                else
                    SendCommandErrorMsg(state, message);
            }
            else if (message == CommandSignal.STOP_REC.ToString())
            {
                if(state == SnifferState.RECORDING)
                    _snifferCore.Stop();
                else
                    SendCommandErrorMsg(state, message);
            }
            else if (message == CommandSignal.REPLAY.ToString())
            {
                if(state == SnifferState.IDLE)
                    _snifferCore.Replay();
                else
                    SendCommandErrorMsg(state, message);
            }
            else if (message == CommandSignal.STOP_REPLAY.ToString())
            {
                if(state == SnifferState.PLAYING_BACK)
                    _snifferCore.StopReplay();
                else
                    SendCommandErrorMsg(state,message);
            }
            else
            {
                Debug.Log($"Unknown command: {message} ");
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
            Show();
        }

        private void Show()
        {
            _canvasGroup.alpha = 1;
        }

        public void Hide()
        {
            _canvasGroup.alpha = 0;
        } 

        public void Connect()
        {
            if (GetIp == string.Empty || portInput.text == string.Empty || _canvasGroup.alpha == 0) return;
            _client.StartClient(GetIp,GetPort);
        }
        
        public void SendSignal()
        {
            
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
    }
}
