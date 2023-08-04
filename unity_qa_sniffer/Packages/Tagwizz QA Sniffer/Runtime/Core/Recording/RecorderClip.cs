using System.Collections;
using System.Collections.Generic;
using TagwizzQASniffer.Core.InputSystem;
using UnityEngine;
using UnityEngine.Serialization;

public enum RecorderClipType
{
   IDLE,
   EVENT_ACTION,
   CONTINUOUS_ACTION
};

[System.Serializable]
public class RecorderClip
{
   [SerializeField] public RecorderClipType type;
   [SerializeField] public InputData inputData;
   [SerializeField] public float timelineStart;
   [SerializeField] public float timelineEnd;
}

