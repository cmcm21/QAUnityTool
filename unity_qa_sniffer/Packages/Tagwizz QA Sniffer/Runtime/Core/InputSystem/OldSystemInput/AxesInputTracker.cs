using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using System.Threading.Tasks;
using UnityEngine;

namespace TagwizzQASniffer.Core.InputSystem.OldSystemInput
{
    public enum AxeInputType { KeyOrMouseButton, MouseMovement, JoystickAxis }
    public class AxesInputTracker: InputTracker
    {
        private  Dictionary<string, InputData> _axesRef = new Dictionary<string, InputData>();
        private readonly Dictionary<string, bool> _axesTracking = new Dictionary<string, bool>();

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
                        Name = name,
                        type = GetInputType(inputType)
                    });
                }
                if(!_axesTracking.ContainsKey(name))
                    _axesTracking.Add(name,false);
            }
        }
        public override async void CheckInputs()
        {
            foreach (var axeName in _axesRef.Keys)
            {
                var axisVal = Input.GetAxis(axeName);
                if (axisVal != 0)
                {
                    if (!_axesTracking[axeName])
                    {
                        _axesTracking[axeName] = true;
                        _axesRef[axeName] = OnTrackStarted(_axesRef[axeName]);
                        Debug.Log($"[{GetType()}]Starting Tracking Input : {axeName}"); 
                    }
                    else
                    {
                        _axesRef[axeName].Duration += Time.deltaTime;
                        Debug.Log($"[{GetType()}]Tracking Input : {axeName}"); 
                    }
                }
                else 
                {
                    if (_axesTracking[axeName])
                    {
                        _axesTracking[axeName] = false;
                        OnTrackEnded(_axesRef[axeName]);                
                        Debug.Log($"[{GetType()}]Ending Tracking Input : {axeName}"); 
                    }
                }
            }
        }

        private  void TrackSingleAxe(string axeName)
        {

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