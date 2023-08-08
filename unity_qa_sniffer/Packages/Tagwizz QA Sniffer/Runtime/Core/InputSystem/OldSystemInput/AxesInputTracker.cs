using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PlasticPipe.Server;
using Unity.IO.LowLevel.Unsafe;
using UnityEditor;
using UnityEngine;

namespace TagwizzQASniffer.Core.InputSystem.OldSystemInput
{
    public enum AxeInputType { KeyOrMouseButton, MouseMovement, JoystickAxis }
    public class AxesInputTracker: InputTracker
    {
        //Each axe name can have different input data
        private readonly Dictionary<string, List<InputData>> _axesRef = new Dictionary<string, List<InputData>>();
        private readonly Dictionary<string, AxeInputType> _axesInputType = new Dictionary<string, AxeInputType>();
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
                     _axesRef.Add(name,new List<InputData>());

                if(!_axesInputType.ContainsKey(name))
                    _axesInputType.Add(name,inputType);
                
                if (!_axesTracking.ContainsKey(name))
                    _axesTracking.Add(name,false);
                
                if(!_axesNames.Contains(name))
                    _axesNames.Add(name);
            }
        }
        
        public override void CheckInputs()
        {
            foreach (var axisName in _axesNames)
            {
                if (axisName == "Mouse X" || axisName == "Mouse Y" || axisName == "Mouse ScrollWheel")
                    ReadMouse(axisName);
                else
                    ReadAxeValue(axisName);
            }
        }

        private async void  ReadMouse(string axisName)
        {
            if(axisName == "Mouse ScrollWheel")
                TrackMouseScroll(axisName);
            else if (axisName == "Mouse X" || axisName == "Mouse Y")
                ReadMouseAxles(axisName);
        }

        private void TrackMouseScroll(string axisName)
        {
            if (Input.mouseScrollDelta.y != 0)
            {
                if (!_axesTracking[axisName])
                {
                    StartReadAxis(axisName);
                    _axesRef[axisName].Last().startingScrollDeltaX = Input.mouseScrollDelta.x;
                    _axesRef[axisName].Last().startingScrollDeltaY = Input.mouseScrollDelta.y;
                }
                else
                {
                   ContinueReadAxis(axisName); 
                    _axesRef[axisName].Last().lastScrollDeltaX = Input.mouseScrollDelta.x;
                    _axesRef[axisName].Last().lastScrollDeltaX = Input.mouseScrollDelta.y;
                }  
            }
            else
            {
                if (_axesTracking[axisName])
                {
                    EndReadAxis(axisName);
                }
            }
        }

        private void ReadMouseAxles(string axisName)
        {
            if (Input.GetAxis(axisName) != 0)
            {
                if (!_axesTracking[axisName])
                    StartReadAxis(axisName);
                else
                {
                   if(_axesRef[axisName].Last().duration < 1f)
                       ContinueReadAxis(axisName);
                   else
                       EndReadAxis(axisName);
                }
            }
            else if(_axesTracking[axisName])
            {
                if(_axesRef[axisName].Last().duration < 1f)
                    ContinueReadAxis(axisName);
                else
                    EndReadAxis(axisName);
            }
        }

        private bool CheckDistanceBetweenInputs(string axisName)
        {
            if (_axesRef[axisName].Count == 0) return true;
            var distance = Vector3.Distance(
                Input.mousePosition, _axesRef[axisName].Last().lastPosition);
            
            Debug.Log($"Distance between mouse movement: {distance}"); 
            return distance >= SnifferDefinitions.MIN_DISTANCE_BETWEEN_INPUTS;
        }
        

        private void ReadAxeValue(string axisName)
        {
            var axisVal = Input.GetAxis(axisName);
            if (axisVal != 0)
            {
                if (!_axesTracking[axisName])
                    StartReadAxis(axisName);
                else
                {
                    if(CheckTimeBetweenInputs(axisName,SnifferDefinitions.MIN_FRAMES_BETWEEN_INPUTS))
                        ContinueReadAxis(axisName);
                    else
                    {
                        EndReadAxis(axisName); 
                        StartReadAxis(axisName);
                    }
                }
            }
            else 
            {
                if (_axesTracking[axisName])
                    EndReadAxis(axisName);
            }
        }

        private bool CheckTimeBetweenInputs(string axisName, float minFrames)
        {
            var frameDifference = Time.frameCount -  _axesRef[axisName].Last().lastFrame;
            return (frameDifference < minFrames);
        }

        private void StartReadAxis(string axisName)
        {
            _axesTracking[axisName] = true;
            _axesRef[axisName].Add(CreateInputData(axisName));
            var lastIndex = _axesRef[axisName].Count - 1;
            _axesRef[axisName][lastIndex] = OnTrackStarted(_axesRef[axisName][lastIndex]);
            _axesRef[axisName][lastIndex].startingAxeValue = Input.GetAxis(axisName);
            _axesRef[axisName][lastIndex].duration += Time.deltaTime;
            Debug.Log($"[{GetType()}]Starting Tracking Input : {axisName}"); 
        }

        private InputData CreateInputData(string axisName)
        {
            return new InputData()
            {
                name = axisName,
                type = GetInputType(_axesInputType[axisName])
            };
        }

        private void ContinueReadAxis(string axisName)
        {
            var lastIndex = _axesRef[axisName].Count - 1;
            _axesRef[axisName][lastIndex].duration += Time.deltaTime;
            _axesRef[axisName][lastIndex].lastAxeValue = Input.GetAxis(axisName);
            _axesRef[axisName][lastIndex].lastFrame = Time.frameCount;
            _axesRef[axisName][lastIndex].lastPosition = Input.mousePosition;
            Debug.Log($"[{GetType()}]Tracking Input : {axisName}"); 
        }

        private void EndReadAxis(string axisName)
        {
            var lastIndex = _axesRef[axisName].Count - 1;
            _axesTracking[axisName] = false;
            _axesRef[axisName][lastIndex].duration += Time.deltaTime;
            OnTrackEnded(_axesRef[axisName].Last());                
            Debug.Log($"[{GetType()}]Ending Tracking Input : {axisName}"); 
        }

        private string GetInputType(AxeInputType type)
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
            return inputType.ToString();
        }
    }

    public struct ReadInput
    {
        public string Name;
        public int ReadFrame;
    }
}