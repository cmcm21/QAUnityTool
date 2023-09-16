using System;
using TagwizzQASniffer.Core;

namespace TagwizzQASniffer.Core.Recording
{
    public interface IRecorder: ISnifferObserverSubscriber
    {
        public bool Subscribe(IRecorderListener listener);
        public void Unsubscribe(IRecorderListener listener);

        public int GetReplayPosition();
        public int GetRecordingLength();
        public void StartRec();

        public void StopRec();

        public void SaveToFile(string fileName);

        public void Replay();

        public void StopPlay();

        public void ReplayOneStep();

        public void LoadFromFile(string fileName);
        public void Clear();

        public void Pause();
    }

    public interface IRecorderListener {
        public void OnRecordStarted();
        public void OnRecordFinished();
        public void OnReplayStarted();
        public void OnReplayFinished();
        public void OnReplayStepByStepStarted();
    }
}
