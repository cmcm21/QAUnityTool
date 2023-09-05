using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

namespace TagwizzQASniffer.Network
{
    
    public enum StreamingClientState {SENDING_FRAME,IDLE}
    public enum StreamingMessages {STARTING, FINISHING}
    public class StreamingClient
    {
        private const int STREAMING_PORT = 7777;
        private const int BUFFER_SIZE = 32754;
        private IPAddress[] _ipArray;
        private IPEndPoint _endPoint;
        private Socket _sender;
        private readonly Queue<MemoryStream> _framesToSend = new Queue<MemoryStream>();
        private Thread _socketThread;
        private StreamingClientState _state;
        private bool _canSend = false;
        private string _ip;
        private int _frameCounter = 0;

        public StreamingClient()
        {
            _state = StreamingClientState.IDLE;
        }

        public void StartClient(string ip)
        {
            _socketThread = new Thread(o => ConnectToStreamingServer(ip,STREAMING_PORT))
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
                _sender.Close();
            } 
        }
        
        private bool SendFrame(MemoryStream stream)
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
                   if (_sender.Send(frameChunk, numBytes, SocketFlags.None) != numBytes) {
                       throw new Exception("Error in sending the frame data");
                   }
                   bytesSoFar += numBytes;
                   Byte progress = (byte)(bytesSoFar * 100 / totalBytes);
                   if (progress > lastStatus && progress != 100) {
                       //Debug.LogFormat("Frame sending progress:{0}", lastStatus);
                       lastStatus = progress;
                   }
               }
               //Debug.Log($"Frame {_frameCounter} sending process finished");
            } 
            catch (SocketException e)
            {
                Debug.LogFormat("Socket exception: {0}", e.Message.ToString());
                return false;
            } 
            finally 
            { 
                stream?.Close();
                _frameCounter++;
               _state = StreamingClientState.IDLE;
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
