using System.Globalization;
using UnityEngine;
using UnityEngine.Serialization;

namespace TagwizzQASniffer.Core.InputSystem
{
    [System.Serializable]
    public class InputData
    {
        public InputData()
        {
            type = InputType.KEY;
            name = "";
            duration = 0;
            startingPosition = Vector2.zero;
            endingPosition = Vector2.zero;
            startingTime = 0;
            endingTime = 0;
            startingFrame = 0;
            endingFrame = 0;
            axeValue = 0;
        }
        [SerializeField] public InputType type;
        [SerializeField] public string name;
        [SerializeField] public float duration;
        [SerializeField] public Vector2 startingPosition;
        [SerializeField] public Vector2 endingPosition;
        [SerializeField] public float startingTime;
        [SerializeField] public float endingTime;
        [SerializeField] public int startingFrame;
        [SerializeField] public int endingFrame;
        [SerializeField] public float axeValue;
        public string Id => name + startingTime.ToString(CultureInfo.InvariantCulture);
    }
}