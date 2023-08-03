using UnityEngine;

namespace TagwizzQASniffer.Core.InputSystem
{
    public class InputData
    {
        public InputData()
        {
            type = InputType.KEY;
            Name = "";
            Duration = 0;
            StartingPosition = Vector2.zero;
            EndingPosition = Vector2.zero;
            StartingTime = 0;
            EndingTime = 0;
            startingFrame = 0;
            endingFrame = 0;
            axeValue = 0;
        }
        
        public InputType type;
        public string Name;
        public float Duration;
        public Vector2 StartingPosition;
        public Vector2 EndingPosition;
        public float StartingTime;
        public float EndingTime;
        public int startingFrame;
        public int endingFrame;
        public float axeValue;
    }
}