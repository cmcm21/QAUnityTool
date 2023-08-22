using System;
using TagwizzQASniffer.Core;

namespace TagwizzQASniffer.Core.Recording
{
    public interface IRecorder: ISnifferObserverSubscriber
    {
        public event Action OnRecordStarted;
        public event Action OnRecordFinished;
        public event Action OnReplayStarted;
        public event Action OnReplayFinished;
        public int GetRecLenght();
        public int GetRecPosition();
        public void StartRec();

        public void StopRec();

        public void SaveToFile(string fileName);

        public void Replay();

        public void StopPlay();

        public void LoadFromFile(string fileName);
        public void Clear();

        public void Pause();
    }
}
