using TagwizzQASniffer.Core;

namespace TagwizzQASniffer.Core.Recording
{
    public interface IRecorder: ISnifferObserverSubscriber
    {
        public int GetRecLenght();
        public int GetRecPosition();
        public void StartRec();

        public void StopRec();

        public void SaveToFile(string fileName);

        public void Play();

        public void StopPlay();

        public void LoadFromFile(string fileName);
        public void Clear();

        public void Pause();
    }
}