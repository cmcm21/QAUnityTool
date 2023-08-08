using System.Collections.Generic;
using System.Linq;
using TagwizzQASniffer.Core.InputSystem;
using UnityEngine;

namespace TagwizzQASniffer.Core.Recording
{
    public class RecorderTimeline
    {
        private float _duration;
        private readonly string _name;
        private readonly Dictionary<string, RecorderClip> _clipsRefs = new Dictionary<string, RecorderClip>();

        public RecorderTimeline(string name)
        {
            _name = name == string.Empty ? "sniffer_rec" : name;
            _duration = 0;
        }
        
        public void Load(RecordingData recordingData)
        {
            _duration = recordingData.timelineDuration;
            foreach(var clip in recordingData.clips)
                if(!_clipsRefs.ContainsKey(clip.inputData.Id))
                    _clipsRefs.Add(clip.inputData.Id,clip);
        }

        public void ClipStarted(InputData inputData)
        {
            if (!_clipsRefs.ContainsKey(inputData.Id))
            {
                _clipsRefs.Add(inputData.Id,new RecorderClip()
                {
                    inputData = inputData,
                    timelineStart = _duration
                }); 
            }
        }

        public void ClipFinished(InputData inputData)
        {
            if (_clipsRefs.TryGetValue(inputData.Id, out var clip))
            {
                clip.timelineEnd = _duration;
                clip.type = GetRecorderType(inputData.duration);
            }
        }

        private string GetRecorderType(float inputDuration)
        {
            var duration = inputDuration;
            if (duration <= SnifferDefinitions.EVENT_TIME || duration < SnifferDefinitions.CONTINUOUS_TIME)
                return RecorderClipType.EVENT_ACTION.ToString();
            if (duration >= SnifferDefinitions.CONTINUOUS_TIME)
                return RecorderClipType.CONTINUOUS_ACTION.ToString();
            
            return RecorderClipType.IDLE.ToString();
        }
        
        public void Update()
        {
            _duration += Time.deltaTime;
            Debug.Log($"RecordingTimeline time: {_duration}");
        }

        public RecordingData Export()
        {
            return new RecordingData()
            {
                timelineName = _name,
                timelineDuration = _duration,
                clips = _clipsRefs.Values.ToList()
            };
        }
    }

    [System.Serializable]
    public class RecordingData
    {
        [SerializeField] public string timelineName;
        [SerializeField] public float timelineDuration;
        [SerializeField] public List<RecorderClip> clips;
    }
}