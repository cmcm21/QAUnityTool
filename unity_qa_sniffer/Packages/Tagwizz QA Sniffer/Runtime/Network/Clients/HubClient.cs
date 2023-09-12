using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using TagwizzQASniffer.Exceptions;
using TagwizzQASniffer.Network.Clients;
using UnityEngine;

namespace TagwizzQASniffer.Network
{
    public class HubClient
    {
        volatile bool _isReading = false;
        public bool isReading => _isReading;
        private Thread _socketThread;
        private Socket _sender;
        public event Action<string> OnReceivedMsgFromServerEvent;
        private readonly ClientObserver _observer = new ClientObserver();
        public ClientObserver Observer => _observer;

        public void SendMsgToServer( string msg )
        {
            try
            {
                if (_sender != null && _sender.Connected)
                {
                    Debug.Log("SendMsgToServer");
                    byte[] messageSent = Encoding.ASCII.GetBytes($"{msg}");
                    int byteSent = _sender.Send(messageSent);
                }
                else
                {
                    Debug.LogFormat("Not able to send: {0}", _sender is { Connected: true });
                }
            }
            catch (SocketException se)
            {
                StopClient();
            }
            catch (ObjectDisposedException oe)
            {
                StopClient();
            }
        }
        
        private void OnReceivedMsgFromServer( string msg ) 
        { 
            Debug.LogFormat("Message from Server -> {0}", msg); 
            OnReceivedMsgFromServerEvent?.Invoke(msg);
        }

        public void StartClient(string ip, int port)
        {
            _socketThread = new Thread(o => ExecuteClient(ip,port))
            {
                IsBackground = true
            };
            _socketThread.Start();
        }
       
        private void ExecuteClient(string ip, int port) 
        { 
            try 
            { 
                _isReading = true;
                IPAddress[] ipArray = Dns.GetHostAddresses(ip);
                IPEndPoint localEndPoint = new IPEndPoint(ipArray[0], port);
                _sender = new Socket(ipArray[0].AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                try
                {
                    _sender.Connect(localEndPoint);
                    Debug.LogFormat("Socket connected to -> {0} ", _sender.RemoteEndPoint.ToString());
                    _observer.ConnectedNotify();
                    while (_isReading)
                    {
                        byte[] messageReceived = new byte[1024];
                        int bytesReceive = _sender.Receive(messageReceived);
                        OnReceivedMsgFromServer(Encoding.ASCII.GetString(messageReceived, 0, bytesReceive));
                    }
                    _sender.Shutdown(SocketShutdown.Both);
                }
                catch (ArgumentNullException ane)
                {
                    Debug.LogFormat("ArgumentNullException : {0}", ane.ToString());
                }
                catch (SocketException se)
                {
                    Debug.LogFormat("SocketException : {0}", se.ToString());
                }
                catch (Exception e)
                {
                    Debug.LogFormat("Unexpected exception : {0}", e.ToString());
                }
                finally
                {
                    StopClient();
                }
            } 
            catch (Exception e)
            { 
                Debug.LogFormat(e.ToString()); 
                StopClient();
            } 
        }

         
        public void StopClient()
        {
            if (!_isReading && !_sender.Connected) return;
            
            _isReading = false;
            if (_socketThread != null)
                _socketThread.Abort();

            try {
                
                if(_sender != null)
                    _sender?.Shutdown(SocketShutdown.Both);
            }
            catch (ObjectDisposedException ode)
            {
                Debug.Log($"Sender object is already disposed, {ode}");
            }
            finally {
                if (_sender != null)
                {
                    _sender.Close(); 
                    _observer.DisconnectedNotify(); 
                }
            }
            
        }
        
        ~HubClient()
        {
            if (_sender != null)
                StopClient();
        }
    }
}
