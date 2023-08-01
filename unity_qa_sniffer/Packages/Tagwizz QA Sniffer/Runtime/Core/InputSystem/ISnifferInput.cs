namespace Tagwizz_QA_Sniffer.Core.InputSystem
{
    public delegate void InputEventTriggered(InputData inputData);
    public abstract class ISnifferInput
    {
        private event InputEventTriggered InputEvent;
        public virtual void Init(){}
        public virtual void GetInputs(){}

        protected void OnInputEvent(InputData inputData)
        {
            InputEvent?.Invoke(inputData);
        }
    }
    
    public enum InputType{TOUCH,KEY,BUTTON}
}