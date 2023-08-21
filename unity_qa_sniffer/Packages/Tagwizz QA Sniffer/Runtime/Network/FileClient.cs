using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using TagwizzQASniffer.Core;
using UnityEngine;

public class FileClient
{
    private System.Threading.Thread _socketThread;
    private IPAddress[] _ipArray;
    private IPEndPoint _endPoint;
    private Socket _sender;
    
    public void SaveFile(string ip, int port, SnifferCore snifferCore)
    {
        _socketThread = new System.Threading.Thread(o => SendFileThread(ip, port, snifferCore))
         {
             IsBackground = true
         };
        _socketThread.Start(); 
    }

    public void LoadFile(string ip, int port, SnifferCore snifferCore)
    {
         var recordingFile = string.Empty;
         _socketThread = new System.Threading.Thread(o => recordingFile = GetFileThread(ip, port, snifferCore))
          {
              IsBackground = true
          };
         _socketThread.Start();
         _socketThread.Join();

         if (recordingFile != string.Empty)
         {
            snifferCore.Load(recordingFile);
            Debug.Log($"Sniffer core file {recordingFile} loaded");
         }
    }
    
    private bool SendFileThread(string ip, int port, SnifferCore snifferCore)
    {
        _ipArray = Dns.GetHostAddresses(ip);
        _endPoint = new IPEndPoint(_ipArray[0], port + 100);
        FileStream file = null;
        _sender = new Socket(_ipArray[0].AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        //_sender.SendTimeout = 1000000; //timeout in milliseconds
        try {
            _sender.Connect(_endPoint);
             Debug.LogFormat("File Socket connected to -> {0} ", _sender.RemoteEndPoint.ToString()); 
             var filePath = SetFileToSend(snifferCore); 
             int lastStatus = 0;
             file = new FileStream(filePath, FileMode.Open);;
             long totalBytes = file.Length, bytesSoFar = 0;
             byte[] fileChunk = new byte[4096];
            var fileSizeMsgBytes = Encoding.ASCII.GetBytes($"{file.Length}");
             Debug.Log($"File {filePath} Size: {file.Length}");
            _sender.Send(fileSizeMsgBytes); 
            
            int numBytes;
            while ((numBytes = file.Read(fileChunk, 0, 4096)) > 0)
            {
                if (_sender.Send(fileChunk, numBytes, SocketFlags.None) != numBytes) {
                    throw new Exception("Error in sending the file");
                }
                bytesSoFar += numBytes;
                Byte progress = (byte)(bytesSoFar * 100 / totalBytes);
                if (progress > lastStatus && progress != 100) {
                    Debug.LogFormat("File sending progress:{0}", lastStatus);
                    lastStatus = progress;
                }
            }    
            
            Debug.Log("File sending process finished");
            _sender.Shutdown(SocketShutdown.Both);
        } catch (SocketException e) {
            Debug.LogFormat("Socket exception: {0}", e.Message.ToString());
            return false;
        } finally {
            file?.Close();
            _sender.Close();
        }
        return true;
    }    
    
     private string GetFileThread(string ip, int port, SnifferCore snifferCore)
    {
        _ipArray = Dns.GetHostAddresses(ip);
        _endPoint = new IPEndPoint(_ipArray[0], port + 100);
        FileStream file = null;
        _sender = new Socket(_ipArray[0].AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        //_sender.SendTimeout = 1000000; //timeout in milliseconds
        try 
        {
             _sender.Connect(_endPoint);
             Debug.LogFormat("File Socket connected to -> {0} ", _sender.RemoteEndPoint.ToString()); 
             byte[] fileNameBytes = new byte[1024];
             int received = _sender.Receive(fileNameBytes);
             var fileName = Encoding.ASCII.GetString(fileNameBytes, 0, received);
             var filePath = GetFilePath(fileName);
             file = new FileStream(filePath, FileMode.Create);;
             Debug.Log($"Receiving File {fileName}");
            
             while(true)
             {
                 byte[] fileBytes = new byte[4096];
                 var bytes = _sender.Receive(fileBytes);
                 
                 if (bytes == 0)
                     break;
                 Debug.Log($"File data {fileBytes.Length} received");
                 file.Write(fileBytes,0,fileBytes.Length);
             }    
             Debug.Log("File sending process finished");
             _sender.Shutdown(SocketShutdown.Both);
        } catch (SocketException e) {
            Debug.LogFormat("Socket exception: {0}", e.Message.ToString());
            return "";
        } finally {
            file?.Close();
            _sender.Close();
        }
        return file.Name;
    }
    
    private string SetFileToSend(SnifferCore snifferCore)
    {
        byte[] fileNameBytes = new byte[1024];
        int received = _sender.Receive(fileNameBytes);
        var fileName = Encoding.ASCII.GetString(fileNameBytes, 0, received);
        Debug.Log($"File name received: {fileName}");
        var filePath = GetFilePath(fileName); 
        snifferCore.Save(filePath);

        return filePath;
    }

    private string GetFilePath(string fileName)
    {
         #if UNITY_EDITOR   
         return SnifferDefinitions.RECORDINGS_PATH + fileName;
         #else
         return Application.persistentDataPath + fileName;
         #endif
    }
    
    
    public void StopClient() 
    { 
        if (_socketThread != null)
            _socketThread.Abort();

        try {
            _sender?.Shutdown(SocketShutdown.Send);
        }
        finally {
            _sender?.Close(); 
        }
    }
}
