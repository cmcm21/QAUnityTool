using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using TagwizzQASniffer.Core;
using UnityEngine;

namespace TagwizzQASniffer.Network
{
    
    public enum StreamingClientState {SENDING_FRAME,IDLE}
    public class StreamingClient
    {
        private const int STREAMING_PORT = 3333;
        private const int BUFFER_SIZE = 32754;
        private IPAddress[] _ipArray;
        private IPEndPoint _endPoint;
        private Socket _sender;
        private Queue<MemoryStream> _framesToSend = new Queue<MemoryStream>();
        private Thread _socketThread;
        private StreamingClientState _state;
        private bool _canSend = false;
        private string _ip;

        public StreamingClient()
        {
            _state = StreamingClientState.IDLE;
        }

        public void SetAddress(string ip)
        {
            _ip = ip;
        }
        
        private void StartSocket(string ip, MemoryStream stream )
        {
            _socketThread = new Thread(o => SendFrame(ip, STREAMING_PORT, stream))
            {
                IsBackground = true
            };
            _socketThread.Start(); 
        }

        public void AddFrameToSend(MemoryStream stream)
        {
            _framesToSend.Enqueue(stream);

            if (_state == StreamingClientState.IDLE)
            {
                var frame = _framesToSend.Dequeue(); 
                StartSocket(_ip, frame);
            }
        }

        private void ConnectToStreamingServer(string ip, int port)
        {
           _canSend = true;
            
            try
            {
                while (_canSend)
                {
                    if(_framesToSend.Count > 0 &&  _state == StreamingClientState.IDLE)
                        SendFrame(_ip, STREAMING_PORT, _framesToSend.Dequeue());
                }
            }
            catch (SocketException e) {
                Debug.LogFormat("Socket exception: {0}", e.Message.ToString());
            } 
            finally {
                _sender.Close();
            } 
        }
    
        private bool SendFrame(string ip, int port, MemoryStream stream)
        {
            try
            {
                _state = StreamingClientState.SENDING_FRAME;
                _ipArray = Dns.GetHostAddresses(ip);
                _endPoint = new IPEndPoint(_ipArray[0], port);
                _sender = new Socket(_ipArray[0].AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                _sender.Connect(_endPoint);
                Debug.LogFormat("File Socket connected to -> {0} ", _sender.RemoteEndPoint.ToString());

                using (var newStream = new MemoryStream(stream.ToArray()))
                {
                     int lastStatus = 0;
                     long totalBytes = newStream.Length, bytesSoFar = 0;
                     byte[] frameChunk = new byte[BUFFER_SIZE];
                     int numBytes;
                     
                     while ((numBytes = newStream.Read(frameChunk, 0, BUFFER_SIZE)) > 0)
                     {
                         if (_sender.Send(frameChunk, numBytes, SocketFlags.None) != numBytes) {
                             throw new Exception("Error in sending the frame data");
                         }
                         bytesSoFar += numBytes;
                         Byte progress = (byte)(bytesSoFar * 100 / totalBytes);
                         if (progress > lastStatus && progress != 100) {
                             Debug.LogFormat("Frame sending progress:{0}", lastStatus);
                             lastStatus = progress;
                         }
                     }    
                }
                Debug.Log("File sending process finished");
                _sender.Shutdown(SocketShutdown.Both);
            } 
            catch (SocketException e)
            {
                Debug.LogFormat("Socket exception: {0}", e.Message.ToString());
                return false;
            } 
            finally 
            { 
                stream?.Close();
                _sender.Close();
                _state = StreamingClientState.IDLE;
            }

            if (_framesToSend.Count > 0)
            {
                var frame = _framesToSend.Dequeue();
                SendFrame(ip, STREAMING_PORT, frame);
            }

            return true;
        }

        public void StopSocket()
        {
            _canSend = false;
            if(_socketThread != null)
                _socketThread.Abort();
        }
    }
}
