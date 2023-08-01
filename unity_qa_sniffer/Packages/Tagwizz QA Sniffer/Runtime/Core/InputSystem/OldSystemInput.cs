using UnityEditor;
using UnityEngine;

namespace Tagwizz_QA_Sniffer.Core.InputSystem
{
    public class OldSystemInput: ISnifferInput
    {
        public override void Init()
        {
        }

        public override void GetInputs()
        {
            if (Input.anyKey)
            {
                
            }

            if (Input.anyKeyDown)
            {
                
            }
        }

        private InputData CreateInputDataFromKey()
        {
            return new InputData()
            {
                type = InputType.KEY,
                Name = Input.inputString,
                StartingPosition = new Vector2(0,0),
                EndingPosition = new Vector2(0,0),
            };
        }
        
        
    }
}