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
            type = "";
            name = "";
            duration = 0;
            startingPosition = Vector2.zero;
            endingPosition = Vector2.zero;
            startingFrame = 0;
            endingFrame = 0;
            startingAxeValue = 0;
            lastAxeValue = 0;
            startingScrollDeltaY = 0;
            startingScrollDeltaX = 0;
            lastScrollDeltaX = 0;
            lastScrollDeltaY = 0;
        }
        [SerializeField] public string type;
        [SerializeField] public string name;
        [SerializeField] public float duration;
        [SerializeField] public Vector2 startingPosition;
        [SerializeField] public Vector2 lastPosition;
        [SerializeField] public Vector2 endingPosition;
        [SerializeField] public float startingTime;
        [SerializeField] public float endingTime;
        [SerializeField] public int startingFrame;
        [SerializeField] public int lastFrame;
        [SerializeField] public int endingFrame;
        [SerializeField] public float startingAxeValue;
        [SerializeField] public float lastAxeValue;
        [SerializeField] public float startingScrollDeltaX;
        [SerializeField] public float startingScrollDeltaY;
        [SerializeField] public float lastScrollDeltaX;
        [SerializeField] public float lastScrollDeltaY;
        public string Id => name + startingFrame.ToString(CultureInfo.InvariantCulture);
    }
}