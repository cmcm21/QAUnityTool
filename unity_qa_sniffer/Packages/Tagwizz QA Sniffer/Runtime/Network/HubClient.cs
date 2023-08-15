using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using UnityEngine;

namespace TagwizzQASniffer.Network
{
    public class HubClient
    {
        volatile bool _keepReading = false;
        private System.Threading.Thread _socketThread;
        private Socket _sender;

        public void SendMsgToServer( string msg ) { 
            if( _sender != null && _sender.Connected ) { 
                Debug.Log("SendMsgToServer");
                byte[] messageSent = Encoding.ASCII.GetBytes(string.Format("{0}", msg)); 
                int byteSent = _sender.Send(messageSent);    
            }
            else { 
                Debug.LogFormat("Not able to send: {0}", _sender.Connected);
            }
        }
        
        void OnReceivedMsgFromServer( string msg ) { 
            Debug.LogFormat("Message from Server -> {0}", msg); 
        }

        public void StartClient(string ip, int port)
        {
            _socketThread = new System.Threading.Thread(o => ExecuteClient(ip,port))
            {
                IsBackground = true
            };
            _socketThread.Start();
        }
        
        public void StopClient() { 
            _keepReading = false;

            if (_socketThread != null)
            {
                _socketThread.Abort();
            }
            
            _sender?.Shutdown(SocketShutdown.Both); 
            _sender?.Close(); 
        }
        
        private void ExecuteClient(string ip, int port) 
        { 
            try 
            { 
                _keepReading = true;
                IPAddress[] ipArray = Dns.GetHostAddresses(ip);
                IPEndPoint localEndPoint = new IPEndPoint(ipArray[0], port);
        
                _sender = new Socket(ipArray[0].AddressFamily, 
                        SocketType.Stream, ProtocolType.Tcp); 
        
                try 
                { 
                    _sender.Connect(localEndPoint); 
                    Debug.LogFormat("Socket connected to -> {0} ", _sender.RemoteEndPoint.ToString()); 
                        
                    while(_keepReading) 
                    { 
                        byte[] messageReceived = new byte[1024]; 
                        int byteRecv = _sender.Receive(messageReceived); 
                        OnReceivedMsgFromServer( Encoding.ASCII.GetString(messageReceived, 0, byteRecv));
                    }  
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
            } 
            catch (Exception e)
            { 
                Debug.LogFormat(e.ToString()); 
            } 
        } 
    }
}
