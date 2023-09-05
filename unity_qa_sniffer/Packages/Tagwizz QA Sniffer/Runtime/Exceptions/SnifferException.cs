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

    public class SnifferCoreLoadingFileError : Exception
    {
        public SnifferCoreLoadingFileError(){}
        public SnifferCoreLoadingFileError(string filePath): 
            base($"Sniffer core couldn't load file {filePath}, file is corrupted"){}
    }

    public class SnifferCoreSavingFileError : Exception
    {
        public SnifferCoreSavingFileError() {}
        public SnifferCoreSavingFileError(string filePath):
            base($"Sniffer core couldn't save file {filePath}"){}
    }
}
