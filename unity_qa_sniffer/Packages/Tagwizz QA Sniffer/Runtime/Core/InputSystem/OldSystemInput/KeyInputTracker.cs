using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Codice.Client.Common.WebApi;
using UnityEngine;

namespace TagwizzQASniffer.Core.InputSystem.OldSystemInput
{
    public class KeyInputTracker: InputTracker
    {
        private readonly Dictionary<string, KeyCode> _keyCodesRef = new Dictionary<string, KeyCode>();
        public KeyInputTracker()
        {
            foreach(KeyCode keycode in Enum.GetValues(typeof(KeyCode)))
            {
                if(keycode == KeyCode.None)continue;
                
                if(!InputDataRead.ContainsKey(keycode.ToString()))
                    InputDataRead.Add(keycode.ToString(),new List<InputData>());
                
                if(!InputsNames.Contains(keycode.ToString()))
                    InputsNames.Add(keycode.ToString());
                
                if(!TrackingInputs.ContainsKey(keycode.ToString()))
                    TrackingInputs.Add(keycode.ToString(),false);
                
                if(!_keyCodesRef.ContainsKey(keycode.ToString()))
                    _keyCodesRef.Add(keycode.ToString(),keycode); 
            }
        }

        public override async void CheckInputs()
        {
            if (!Input.anyKey) return;
            
            foreach (var key in InputsNames)
            {
                if (Input.GetKey(_keyCodesRef[key]))
                {
                    if(!TrackingInputs[key])
                        StartTracking(key);
                    else
                        ContinueTracking(key);
                }
                else
                {
                    if(TrackingInputs[key]) 
                        EndTracking(key);
                }
            }
        }

        protected override void StartTracking(string inputName)
        {
            base.StartTracking(inputName);
            InputDataRead[inputName].Last().type = InputType.KEY.ToString();
        }
    }
}