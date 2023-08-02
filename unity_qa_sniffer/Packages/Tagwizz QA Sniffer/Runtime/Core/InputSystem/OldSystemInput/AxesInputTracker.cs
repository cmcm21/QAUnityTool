using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using System.Threading.Tasks;
using UnityEngine;

namespace Tagwizz_QA_Sniffer.Core.InputSystem.OldSystemInput
{
    public enum AxeInputType { KeyOrMouseButton, MouseMovement, JoystickAxis }
    public class AxesInputTracker: InputTracker
    {
        private readonly Dictionary<string, AxeData> _axesRef = new Dictionary<string, AxeData>();

        public AxesInputTracker()
        {
            var inputManager = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/InputManager.asset")[0];
            
            var inputManagerSer = new SerializedObject(inputManager);
            var axisArray = inputManagerSer.FindProperty("m_Axes");

            for (int i = 0; i < axisArray.arraySize; i++)
            {
                var axis = axisArray.GetArrayElementAtIndex(i);

                var name = axis.FindPropertyRelative("m_Name").stringValue;
                var axisVal = axis.FindPropertyRelative("axis").intValue;
                var inputType = (AxeInputType)axis.FindPropertyRelative("type").intValue;
                
                _axesRef.Add(name,new AxeData()
                {
                    Value =  axisVal,
                    Name = name,
                    Type = inputType
                });
            }
        }

        public override async void CheckInputs()
        {
            foreach (var axeName in _axesRef.Keys)
            {
                if (Input.GetAxis(axeName) != 0)
                {
                   //TODO: start a new task to track this input axeName and after tracking send inputData  
                }
            }
        }
    }
    
    public struct AxeData
    {
        public string Name;
        public int Value;
        public AxeInputType Type;
    }
}