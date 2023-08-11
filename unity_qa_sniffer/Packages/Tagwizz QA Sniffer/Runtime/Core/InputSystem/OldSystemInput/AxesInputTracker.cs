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
        private readonly Dictionary<string, AxeInputType> _axesInputType = new Dictionary<string, AxeInputType>();

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
                
                if (!InputDataRead.ContainsKey(name))
                     InputDataRead.Add(name,new List<InputData>());

                if(!_axesInputType.ContainsKey(name))
                    _axesInputType.Add(name,inputType);
                
                if (!TrackingInputs.ContainsKey(name))
                    TrackingInputs.Add(name,false);
                
                if(!InputsNames.Contains(name))
                    InputsNames.Add(name);
            }
        }
        
        public override void CheckInputs()
        {
            foreach (var axisName in InputsNames)
            {
                if (axisName == "Mouse X" || axisName == "Mouse Y" || axisName == "Mouse ScrollWheel")
                   ReadMouse(axisName);
                else
                    ReadAxeValue(axisName);
            }
        }

        private  void  ReadMouse(string axisName)
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
                if (!TrackingInputs[axisName])
                {
                    StartTracking(axisName);
                    InputDataRead[axisName].Last().startingScrollDeltaX = Input.mouseScrollDelta.x;
                    InputDataRead[axisName].Last().startingScrollDeltaY = Input.mouseScrollDelta.y;
                }
                else
                {
                   ContinueTracking(axisName); 
                    InputDataRead[axisName].Last().lastScrollDeltaX = Input.mouseScrollDelta.x;
                    InputDataRead[axisName].Last().lastScrollDeltaX = Input.mouseScrollDelta.y;
                }  
            }
            else
            {
                if (TrackingInputs[axisName])
                {
                    if(InputDataRead[axisName].Last().duration < 1f)
                        ContinueTracking(axisName);
                    else
                        EndTracking(axisName);
                }
            }
        }

        private void ReadMouseAxles(string axisName)
        {
            if (Input.GetAxis(axisName) != 0)
            {
                if (!TrackingInputs[axisName])
                    StartTracking(axisName);
                else
                   ContinueTracking(axisName);
            }
            else if(TrackingInputs[axisName])
            {
                if(InputDataRead[axisName].Last().duration < 1f)
                    ContinueTracking(axisName);
                else
                    EndTracking(axisName);
            }
        }

        private void ReadAxeValue(string axisName)
        {
            var axisVal = Input.GetAxis(axisName);
            if (axisVal != 0)
            {
                if (!TrackingInputs[axisName])
                    StartTracking(axisName);
                else
                    ContinueTracking(axisName);
            }
            else 
            {
                if (TrackingInputs[axisName])
                    EndTracking(axisName);
            }
        }

        protected override void StartTracking(string axisName)
        {
            base.StartTracking(axisName);
            
            InputDataRead[axisName].Last().startingAxeValue = Input.GetAxis(axisName);
            InputDataRead[axisName].Last().type = GetInputType(_axesInputType[axisName]);
        }

        protected override void ContinueTracking(string axisName)
        {
            base.ContinueTracking(axisName);
            
            var lastIndex = InputDataRead[axisName].Count - 1;
            InputDataRead[axisName][lastIndex].lastAxeValue = Input.GetAxis(axisName);
        }

        private string GetInputType(AxeInputType type)
        {
            InputType inputType = InputType.KEY;
            switch (type)
            {
                case AxeInputType.JoystickAxis:
                    inputType =  InputType.TOUCH;
                    break;
                case AxeInputType.MouseMovement:
                    inputType =  InputType.TOUCH;
                    break;
                case AxeInputType.KeyOrMouseButton:
                    inputType =  InputType.BUTTON;
                    break;
            }
            return inputType.ToString();
        }
    }
}