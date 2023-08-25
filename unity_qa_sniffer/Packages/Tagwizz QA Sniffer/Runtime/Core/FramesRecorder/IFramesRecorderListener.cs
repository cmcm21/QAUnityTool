using System.IO;

namespace TagwizzQASniffer.Core.FramesRecorder
{
    public interface IFramesRecorderListener
    {
        public void FrameRecorded(MemoryStream stream);
        public void Started();
        public void Stopped();
    }
       
}
