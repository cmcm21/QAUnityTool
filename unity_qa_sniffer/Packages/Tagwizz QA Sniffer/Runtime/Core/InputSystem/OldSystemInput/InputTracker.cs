using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace TagwizzQASniffer.Core.InputSystem.OldSystemInput
{
    public abstract class InputTracker
    {
        protected readonly Dictionary<string, List<InputData>> InputDataRead = new Dictionary<string, List<InputData>>();
        protected readonly Dictionary<string, bool> TrackingInputs = new Dictionary<string, bool>();
        protected readonly List<string> InputsNames = new List<string>();
        public event Action<InputData> TrackStarted;
        public event Action<InputData> TrackEnded;
        public virtual void CheckInputs() {}

        protected virtual void OnTrackStarted(InputData inputData)
        {
            TrackStarted?.Invoke(inputData);
        }

        protected virtual void OnTrackEnded(InputData inputData)
        {
            TrackEnded?.Invoke(inputData);
        }

        protected virtual void StartTracking(string inputName)
        {
            if (!InputDataRead.ContainsKey(inputName)) return;
            
            TrackingInputs[inputName] = true;
            InputDataRead[inputName].Add(CreateInputData(inputName));
            var lastIndex = InputDataRead[inputName].Count - 1;
            InputDataRead[inputName][lastIndex].duration += Time.deltaTime;
            InputDataRead[inputName][lastIndex].startingPosition = Input.mousePosition;
            InputDataRead[inputName][lastIndex].startingFrame = Time.frameCount;
            
            OnTrackStarted(InputDataRead[inputName][lastIndex]);
            Debug.Log($"[{GetType()}]Starting Tracking Input : {inputName}"); 
        }

        protected virtual void ContinueTracking(string inputName)
        {
            if (!InputDataRead.ContainsKey(inputName)) return;
            var lastIndex = InputDataRead[inputName].Count - 1;
            InputDataRead[inputName][lastIndex].duration += Time.deltaTime;
            
            Debug.Log($"[{GetType()}]Tracking Input : {inputName}"); 
        }

        protected virtual void EndTracking(string inputName)
        {
            if (!InputDataRead.ContainsKey(inputName)) return;
    
            var lastIndex = InputDataRead[inputName].Count - 1;
            
            TrackingInputs[inputName] = false;
            InputDataRead[inputName][lastIndex].duration += Time.deltaTime;
            InputDataRead[inputName][lastIndex].endingFrame = Time.frameCount;
            InputDataRead[inputName][lastIndex].endingPosition = Input.mousePosition;
            InputDataRead[inputName][lastIndex].endingFrame = Time.frameCount;
            
            OnTrackEnded(InputDataRead[inputName].Last());                
            Debug.Log($"[{GetType()}]Ending Tracking Input : {inputName}"); 
        }
        
        private InputData CreateInputData(string axisName)
        {
            return new InputData()
            {
                name = axisName,
            };
        }
        
        public void StopTracker()
        {
            foreach(var inputName in InputsNames)
                if(TrackingInputs[inputName])
                    EndTracking(inputName);
        }
        
    }
}