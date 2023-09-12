using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TagwizzQASniffer.Core.FramesRecorder
{
	public enum FrameRecorderState {RECORDING,IDLE, WAITING}
	[RequireComponent(typeof(Camera))]
	public class FrameRecorder : MonoBehaviour 
	{
		// Public Properties
		public int maxFrames; // maximum number of frames you want to record in one video
		public int frameRate = 30; // number of frames to capture per second

		// The Encoder Thread
		private Thread encoderThread;

		// Texture Readback Objects
		private RenderTexture tempRenderTexture;
		private Texture2D tempTexture2D;

		// Timing Data
		private float _captureFrameTime;
		private float _lastFrameTime;
		private int _frameNumber;
		private int _savingFrameNumber;
		private float _frameRate;

		// Encoder Thread Shared Resources
		private Queue<byte[]> _frameQueue;
		private bool _threadIsProcessing;
		private bool _terminateThreadWhenDone;
		
		private readonly FramesRecorderObserver _observer = new FramesRecorderObserver();
		public FramesRecorderObserver Observer => _observer;
		private FrameRecorderState _state;
		public FrameRecorderState State => _state;
	
		private void Start () 
		{
			_state = FrameRecorderState.IDLE;
			Init();
		}

		private void Init()
		{
			_frameQueue = new Queue<byte[]> ();
			_savingFrameNumber = 0;
			_frameRate = 0;

			_captureFrameTime = 1.0f / (float)frameRate;
		}
		
		public void StartRecording()
		{
			_frameNumber = 0;
			// Kill the encoder thread if running from a previous execution
			if (encoderThread != null && (_threadIsProcessing || encoderThread.IsAlive)) {
				_threadIsProcessing = false;
				encoderThread.Join();
			}

			_state = FrameRecorderState.RECORDING;
			// Start a new encoder thread
			_threadIsProcessing = true;
			encoderThread = new Thread (Encode);
			encoderThread.Start ();
			_observer.NotifyStarted();
		}

		public void StopRecording()
		{
			_terminateThreadWhenDone = true;
			_state = FrameRecorderState.IDLE;
			Debug.Log($"<color=red>the last frame recorder was {_frameNumber} frame </color>");
			_observer.NotifyStopped();
		}

		private void OnRenderImage(RenderTexture source, RenderTexture destination)
		{
			if (_state == FrameRecorderState.RECORDING)
			{
				if (_frameNumber <= maxFrames)
				{
					// Calculate number of video frames to produce from this game frame
					// Generate 'padding' frames if desired framerate is higher than actual framerate
					float thisFrameTime = Time.time;
					int framesToCapture = ((int)(thisFrameTime / _captureFrameTime)) -
					                      ((int)(_lastFrameTime / _captureFrameTime));

					if (framesToCapture > 0)
						StartCoroutine(nameof(SaveFrame));

					for (int i = 0; i < framesToCapture && _frameNumber <= maxFrames; ++i)
						_frameNumber++;

					_lastFrameTime = thisFrameTime;
				}
				else //keep making screenshots until it reaches the max frame amount
					_terminateThreadWhenDone = true;
			}
			Graphics.Blit(source, destination);
		}
		
		private IEnumerator SaveFrame()
		{
			// Read the screen buffer after rendering is complete
			yield return new WaitForEndOfFrame();

			// Create a texture in RGB24 format the size of the screen
			int width = Screen.width;
			int height = Screen.height;
			Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);

			// Read the screen contents into the texture
			tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
			tex.Apply();

			// Encode the bytes in JPG format
			byte[] bytes = ImageConversion.EncodeArrayToJPG(
						tex.GetRawTextureData(), 
						tex.graphicsFormat, 
						(uint)width, 
						(uint)height
					  );
			
			Object.Destroy(tex);

			_frameQueue.Enqueue(bytes);
		}

		private void Encode()
		{
			while (_threadIsProcessing) 
			{
				if(_frameQueue.Count > 0)
				{
					var memoryStream = new MemoryStream(_frameQueue.Dequeue());
					_observer.NotifyFrameRecorded(memoryStream);
					_savingFrameNumber++;
				}
				else
				{
					if(_terminateThreadWhenDone)
						break;
					Thread.Sleep(1);
				}
			}
			
			_terminateThreadWhenDone = false;
			_threadIsProcessing = false;
		}
	}
}