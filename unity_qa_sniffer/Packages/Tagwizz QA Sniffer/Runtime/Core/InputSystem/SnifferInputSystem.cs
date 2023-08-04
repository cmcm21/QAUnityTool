namespace TagwizzQASniffer.Core.InputSystem
{
    public delegate void InputEventTriggered(InputData inputData);
    public abstract class SnifferInputSystem
    {
        public event InputEventTriggered InputEvent;
        public virtual void Init() { }
        public virtual void Stop(){}
        public virtual void GetInputs(){}

        public void OnInputEvent(InputData inputData)
        {
            InputEvent?.Invoke(inputData);
        }
        
    }
    
    public enum InputType{TOUCH,KEY,BUTTON}
}