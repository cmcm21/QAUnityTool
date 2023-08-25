using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace TagwizzQASniffer.Core.FramesRecorder
{
    public class FramesRecorderObserver
    {
        private List<IFramesRecorderListener> _subscribers = new List<IFramesRecorderListener>();
        public List<IFramesRecorderListener> Subscribers => _subscribers;
        
        public void Subscribe(IFramesRecorderListener listener)
        {
           _subscribers.Add(listener); 
        }

        public void UnSubscribe(IFramesRecorderListener listener)
        {
            _subscribers.Remove(listener);
        }

        public void NotifyFrameRecorded(MemoryStream stream)
        {
            foreach( var subscriber in _subscribers)
                subscriber.FrameRecorded(stream);
        }

        public void NotifyStarted()
        {
            foreach(var subscriber in _subscribers)
                subscriber.Started();
        }

        public void NotifyStopped()
        {
            foreach(var subscriber in _subscribers)
                subscriber.Stopped();
        }
    }
}
