using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace TagwizzQASniffer.Core.InputSystem.OldSystemInput
{
    public abstract class InputTracker
    {
        public event Action<InputData> TrackStarted;
        public event Action<InputData> TrackEnded;
        public virtual void CheckInputs() {}
        
        protected virtual InputData OnTrackStarted(InputData inputData)
        {
            inputData.startingPosition = Input.mousePosition;
            inputData.startingFrame = Time.frameCount;
            inputData.lastFrame = Time.frameCount;
            TrackStarted?.Invoke(inputData);
            return inputData;
        }

        protected virtual void OnTrackEnded(InputData inputData)
        {
            inputData.endingPosition = Input.mousePosition;
            inputData.endingFrame = Time.frameCount;
            TrackEnded?.Invoke(inputData);
        }

        private bool ValidateInputData(InputData inputData)
        {
            if (inputData.type == InputType.TOUCH.ToString()
                && Vector3.Distance(inputData.startingPosition, inputData.endingPosition) == 0)
                return false;
            
            return true;
        }
    }
}