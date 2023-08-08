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
        private HashSet<string> _keysPressedDown = new HashSet<string>();
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

        public override void CheckInputs()
        {
            CheckAllKeys();
        }

        private void CheckAllKeys()
        {
            foreach (var key in InputsNames)
            {
                if (Input.GetKeyDown(_keyCodesRef[key]) && !TrackingInputs[key])
                {
                    StartTracking(key);
                    _keysPressedDown.Add(key);
                }

                if (Input.GetKeyUp(_keyCodesRef[key]) && TrackingInputs[key])
                {
                    EndTracking(key);
                    _keysPressedDown.Remove(key);
                }
            }
        
            foreach (var key in _keysPressedDown)
            {
                if (Input.GetKey(_keyCodesRef[key]) && TrackingInputs[key])
                    ContinueTracking(key);
                else if(TrackingInputs[key])        
                        EndTracking(key);
            }
        }

        protected override void StartTracking(string inputName)
        {
            base.StartTracking(inputName);
            InputDataRead[inputName].Last().type = InputType.KEY.ToString();
        }
    }
}