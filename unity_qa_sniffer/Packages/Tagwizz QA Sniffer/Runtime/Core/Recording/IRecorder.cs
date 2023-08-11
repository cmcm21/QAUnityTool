using System.Collections;
using System.Collections.Generic;
using TagwizzQASniffer.Core;
using UnityEngine;

public interface IRecorder: ISnifferObserverSubscriber
{
    public void StartRec();

    public void StopRec();

    public void SaveToFile(string fileName);

    public void Play();

    public void StopPlay();

    public void LoadFromFile(string fileName);

}
