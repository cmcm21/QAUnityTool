using System;
using System.Collections;
using System.Collections.Generic;
using TagwizzQASniffer.Network;
using TagwizzQASniffer.Network.Clients;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(NetworkBehaviour))]
public class UINetworkManager : MonoBehaviour,  IClientListener
{
        public enum ListenerState { CONNECTED, DISCONNECTED, ERROR, NONE }
        
        [SerializeField] private TMP_InputField ipInput;
        [SerializeField] private TMP_InputField portInput;
        [SerializeField] private TextMeshProUGUI logger;
        [SerializeField] private Button connectBtn;

        private string GetIp => ipInput.text;
        private int GetPort => int.Parse(portInput.text);

        private NetworkBehaviour _networkBehaviour;
        private ListenerState _listenerState;
        
        public void Start()
        {
            _networkBehaviour = GetComponent<NetworkBehaviour>();
            _networkBehaviour.networkInitialized += OnNetworkInitialized;
            _listenerState = ListenerState.NONE;
            
            SetInput();
            SetPort();
            logger.text = "Disconnected";
        }

        private void OnNetworkInitialized()
        {
            _networkBehaviour.HubClient.Observer.Subscribe(this);
            _networkBehaviour.StreamingClient.Observer.Subscribe(this);
            _listenerState = ListenerState.DISCONNECTED;
        }


        private void SetInput()
        {
            if (PlayerPrefs.HasKey(NetworkDefinitions.LAST_IP_KEY))
            {
                var lastIp = PlayerPrefs.GetString(NetworkDefinitions.LAST_IP_KEY);
                ipInput.text = lastIp;
            }
        }
        
        private void SetPort()
        {
            portInput.text = NetworkDefinitions.DEFAULT_PORT.ToString();
        }


        public void OnConnectClicked()
        {
            if(!_networkBehaviour.Connect(GetIp, GetPort))
                logger.text = $"Error trying to connect to {GetIp}";
            else
            {
                connectBtn.interactable = false; 
                PlayerPrefs.SetString(NetworkDefinitions.LAST_IP_KEY, GetIp);
                PlayerPrefs.Save();
            }
        }

        public void Update()
        {
            if (_listenerState == ListenerState.ERROR)
            {
                logger.text = "Exception from connection, Disconnected";
                connectBtn.interactable = true;
                _listenerState = ListenerState.NONE;
            }
            else if (_listenerState == ListenerState.CONNECTED)
            {
                logger.text = "Connected";
                _listenerState = ListenerState.NONE;
            }
            else if (_listenerState == ListenerState.DISCONNECTED)
            {
                logger.text = "Disconnected";
                connectBtn.interactable = true;
                _listenerState = ListenerState.NONE;
            }
        }


        public void OnDisconnectClicked()
        {
            _networkBehaviour.Disconnect();
            logger.text = "Disconnected";
            connectBtn.interactable = true;
        }

        void IClientListener.Connected()
        {
            _listenerState = ListenerState.CONNECTED;
        }

        void IClientListener.Disconnected()
        {
            _listenerState = ListenerState.DISCONNECTED;
        }

        void IClientListener.ExceptionThrown()
        {
            _listenerState = ListenerState.ERROR;
        }
}
