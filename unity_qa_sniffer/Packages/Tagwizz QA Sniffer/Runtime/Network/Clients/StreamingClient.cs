using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using TagwizzQASniffer.Exceptions;
using TagwizzQASniffer.Network.Clients;
using UnityEngine;

namespace TagwizzQASniffer.Network
{
    
    public enum StreamingClientState {SENDING_FRAME,IDLE}
    public enum StreamingMessages {STARTING, FINISHING}
    public class StreamingClient
    {
        private const int BUFFER_SIZE = 32754;
        private IPAddress[] _ipArray;
        private IPEndPoint _endPoint;
        private Socket _sender;
        private Queue<MemoryStream> _framesToSend;
        private Thread _socketThread;
        private StreamingClientState _state;
        private bool _canSend = false;
        private string _ip;
        private int _frameCounter = 0;
        private ClientObserver _observer;
        public ClientObserver Observer => _observer;

        public StreamingClient()
        {
            _state = StreamingClientState.IDLE;
            _observer = new ClientObserver();
            _framesToSend = new Queue<MemoryStream>();
        }

        public void StartClient(string ip, int streamingPort)
        {
            _ip = ip;
            
            _socketThread = new Thread(o => ConnectToStreamingServer(ip,streamingPort))
            {
                IsBackground = true
            };
            _socketThread.Start(); 
        }

        public void AddFrameToSend(MemoryStream stream)
        {
            _framesToSend.Enqueue(stream);
        }

        private void ConnectToStreamingServer(string ip, int port)
        {
            _canSend = true;
            try
            {
                _ipArray = Dns.GetHostAddresses(ip);
                _endPoint = new IPEndPoint(_ipArray[0], port);
                _sender = new Socket(_ipArray[0].AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                _sender.Connect(_endPoint);
                Debug.LogFormat("File Socket connected to -> {0} ", _sender.RemoteEndPoint.ToString());
                _state = StreamingClientState.IDLE;
                _observer.ConnectedNotify(); 
                while (_canSend)
                {
                    if (_framesToSend.Count > 0 && _state == StreamingClientState.IDLE)
                        SendFrame(_framesToSend.Dequeue());
                }
                _sender.Shutdown(SocketShutdown.Both);
            }
            catch (SocketException e) {
                Debug.LogFormat("Socket exception: {0}", e.Message.ToString());
            } 
            finally {
                StopClient();
            } 
        }
        
        private void SendFrame(MemoryStream stream)
        {
            try
            {
                _state = StreamingClientState.SENDING_FRAME;
                using var newStream = new MemoryStream(stream.ToArray());
                int lastStatus = 0;
                long totalBytes = newStream.Length, bytesSoFar = 0;
                byte[] frameChunk = new byte[BUFFER_SIZE];
                int numBytes;
                var encodeMsg = Encoding.ASCII.GetBytes(StreamingMessages.STARTING.ToString());
                _sender.Send(encodeMsg);

                while ((numBytes = newStream.Read(frameChunk, 0, BUFFER_SIZE)) > 0)
                {
                    if (_sender.Send(frameChunk, numBytes, SocketFlags.None) != numBytes)
                        throw new Exception("Error in sending the frame data");

                    bytesSoFar += numBytes;
                    Byte progress = (byte)(bytesSoFar * 100 / totalBytes);
                    if (progress > lastStatus && progress != 100)
                    {
                        lastStatus = progress;
                    }
                }
            }
            catch (SocketException e)
            {
                StopClient();
                Debug.LogFormat("Socket exception: {0}", e.Message.ToString());
            }
            catch (Exception e)
            {
                StopClient();
                Debug.LogFormat("Exception sending data");
            }
            finally 
            { 
                stream?.Close();
                _frameCounter++;
               _state = StreamingClientState.IDLE;
            }
        }

        public void StopClient()
        {
            _canSend = false;
            _framesToSend.Clear();
            if(_socketThread != null)
                _socketThread.Abort();
            
            try {
                _sender?.Shutdown(SocketShutdown.Send);
            }
            finally {
                _sender?.Close(); 
                _observer.DisconnectedNotify();
            }
            
            _state = StreamingClientState.IDLE;
        }
    }
}
