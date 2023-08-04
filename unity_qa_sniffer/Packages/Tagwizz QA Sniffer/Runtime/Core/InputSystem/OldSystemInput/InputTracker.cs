using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace TagwizzQASniffer.Core.InputSystem.OldSystemInput
{
    public abstract class InputTracker
    {
        public event Action<string> TrackStarted;
        public event Action<InputData> TrackEnded;
        public virtual void CheckInputs() {}

        protected List<Task> _trackTasks = new List<Task>();
        public List<Task> TrackTasks => _trackTasks;

        public virtual async void TrackInputTask() { }
        
        protected virtual InputData OnTrackStarted(InputData inputData)
        {
            inputData.StartingPosition = Input.mousePosition;
            inputData.startingFrame = Time.frameCount;
            Debug.Log($"[{GetType()}]:: Start Tracking Input: {inputData}");
            TrackStarted?.Invoke(inputData.Name);
            return inputData;
        }

        protected virtual void OnTrackEnded(InputData inputData)
        {
            inputData.EndingPosition = Input.mousePosition;
            inputData.endingFrame = Time.frameCount;
            Debug.Log($"[{GetType()}]:: End Tracking Input: {inputData}");
            TrackEnded?.Invoke(inputData);
        }
    }
}