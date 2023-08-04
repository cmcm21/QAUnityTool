using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TagwizzQASniffer.Core.InputSystem.OldSystemInput
{
    public enum AxeInputType { KeyOrMouseButton, MouseMovement, JoystickAxis }
    public class AxesInputTracker: InputTracker
    {
        private readonly Dictionary<string, InputData> _axesRef = new Dictionary<string, InputData>();
        private readonly Dictionary<string, bool> _axesTracking = new Dictionary<string, bool>();
        private readonly List<string> _axesNames = new List<string>();

        public AxesInputTracker()
        {
            var inputManager = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/InputManager.asset")[0];
            var inputManagerSer = new SerializedObject(inputManager);
            var axisArray = inputManagerSer.FindProperty("m_Axes");

            for (int i = 0; i < axisArray.arraySize; i++)
            {
                var axis = axisArray.GetArrayElementAtIndex(i);

                var name = axis.FindPropertyRelative("m_Name").stringValue;
                var inputType = (AxeInputType)axis.FindPropertyRelative("type").intValue;

                if (!_axesRef.ContainsKey(name))
                {
                    _axesRef.Add(name,new InputData()
                    {
                        name = name,
                        type = GetInputType(inputType)
                    });
                    
                    _axesNames.Add(name);
                }
                if(!_axesTracking.ContainsKey(name))
                    _axesTracking.Add(name,false);
            }
        }
        public override void CheckInputs()
        {
            foreach (var axisName in _axesNames)
            {
                var axisVal = Input.GetAxis(axisName);
                if (axisVal != 0)
                {
                    if (!_axesTracking[axisName])
                    {
                        _axesTracking[axisName] = true;
                        _axesRef[axisName] = OnTrackStarted(_axesRef[axisName]);
                        Debug.Log($"[{GetType()}]Starting Tracking Input : {axisName}"); 
                    }
                    else
                    {
                        _axesRef[axisName].duration += Time.deltaTime;
                        Debug.Log($"[{GetType()}]Tracking Input : {axisName}"); 
                    }
                }
                else 
                {
                    if (_axesTracking[axisName])
                    {
                        _axesTracking[axisName] = false;
                        OnTrackEnded(_axesRef[axisName]);                
                        Debug.Log($"[{GetType()}]Ending Tracking Input : {axisName}"); 
                    }
                }
            }
        }

        private InputType GetInputType(AxeInputType type)
        {
            InputType inputType = InputType.KEY;
            switch (type)
            {
                case AxeInputType.JoystickAxis:
                    inputType =  InputType.BUTTON;
                    break;
                case AxeInputType.MouseMovement:
                    inputType =  InputType.TOUCH;
                    break;
                case AxeInputType.KeyOrMouseButton:
                    inputType =  InputType.KEY;
                    break;
            }
            return inputType;
        }
    }
}