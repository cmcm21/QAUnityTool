using System;

namespace TagwizzQASniffer.Exceptions
{
    public class SnifferException : Exception
    {
        public SnifferException() { }
        
        public SnifferException(string filePath): base($"Error while receiving file{filePath}"){}
    }

    public class LoadFileNetworkErrorException : Exception
    {
        public LoadFileNetworkErrorException(){} 
        public LoadFileNetworkErrorException(string filePath): base($"Error while receiving file{filePath}"){}
    }

    public class SaveFileNetworkErrorException : Exception
    {
        public SaveFileNetworkErrorException(){}

        public SaveFileNetworkErrorException(string filePath) : base($"Error while sending file {filePath}") {}
    }

    public class SnifferCoreLoadFileException : Exception
    {
        public SnifferCoreLoadFileException(){}
        public SnifferCoreLoadFileException(string filePath): 
            base($"Sniffer core couldn't load file {filePath}, file is corrupted"){}
    }

    public class SnifferCoreSaveFileException : Exception
    {
        public SnifferCoreSaveFileException() {}
        public SnifferCoreSaveFileException(string filePath):
            base($"Sniffer core couldn't save file {filePath}"){}
    }

    public class NetworkServerConnectionErrorException : Exception
    {
        public NetworkServerConnectionErrorException(){}
        
        public NetworkServerConnectionErrorException(string address, int port):
            base($"Connection to {address} in port : {port} wasn't made"){}
    }

    public class NetworkSendDataErrorException : Exception
    {
       public NetworkSendDataErrorException(){} 
       public NetworkSendDataErrorException(byte[] data, string toIp) : 
           base($"Exception trying to send bytes:{data.Length} to :{toIp}"){}
    }

    public class NetworkReceiveDataErrorException : Exception
    {
        public NetworkReceiveDataErrorException() { }
        
        public NetworkReceiveDataErrorException(string fromIp): 
            base($"Exception trying to receive data from {fromIp} "){}
    }
}
