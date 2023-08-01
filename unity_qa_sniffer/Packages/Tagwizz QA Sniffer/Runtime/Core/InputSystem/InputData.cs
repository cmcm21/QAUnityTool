using UnityEngine;

namespace Tagwizz_QA_Sniffer.Core.InputSystem
{
    public struct InputData
    {
        public InputType type;
        public string Name;
        public float Duration;
        public Vector2 StartingPosition;
        public Vector2 EndingPosition;
        public float StartingTime;
        public float EndingTime;
    }
}