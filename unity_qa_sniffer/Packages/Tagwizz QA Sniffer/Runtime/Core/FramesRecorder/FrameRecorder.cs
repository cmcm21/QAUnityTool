using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;

namespace TagwizzQASniffer.Core.FramesRecorder
{
	class BitmapEncoder
	{
		public static Stream WriteBitmap(Stream stream, int width, int height, byte[] imageData)
		{
			using (BinaryWriter bw = new BinaryWriter(stream)) {

				// define the bitmap file header
				bw.Write ((UInt16)0x4D42); 								// bfType;
				bw.Write ((UInt32)(14 + 40 + (width * height * 4))); 	// bfSize;
				bw.Write ((UInt16)0);									// bfReserved1;
				bw.Write ((UInt16)0);									// bfReserved2;
				bw.Write ((UInt32)14 + 40);								// bfOffBits;
	 
				// define the bitmap information header
				bw.Write ((UInt32)40);  								// biSize;
				bw.Write ((Int32)width); 								// biWidth;
				bw.Write ((Int32)height); 								// biHeight;
				bw.Write ((UInt16)1);									// biPlanes;
				bw.Write ((UInt16)32);									// biBitCount;
				bw.Write ((UInt32)0);  									// biCompression;
				bw.Write ((UInt32)(width * height * 4));  				// biSizeImage;
				bw.Write ((Int32)0); 									// biXPelsPerMeter;
				bw.Write ((Int32)0); 									// biYPelsPerMeter;
				bw.Write ((UInt32)0);  									// biClrUsed;
				bw.Write ((UInt32)0);  									// biClrImportant;

				// switch the image data from RGB to BGR
				for (int imageIdx = 0; imageIdx < imageData.Length; imageIdx += 3) {
					bw.Write(imageData[imageIdx + 2]);
					bw.Write(imageData[imageIdx + 1]);
					bw.Write(imageData[imageIdx + 0]);
					bw.Write((byte)255);
				}
			
			}
			return stream;
		}

	}
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

		// Encoder Thread Shared Resources
		private Queue<byte[]> _frameQueue;
		private int _screenWidth;
		private int _screenHeight;
		private bool _threadIsProcessing;
		private bool _terminateThreadWhenDone;
		
		private readonly FramesRecorderObserver _observer = new FramesRecorderObserver();
		public FramesRecorderObserver Observer => _observer;
		private FrameRecorderState _state;
		public FrameRecorderState State => _state;
	
		private void Start () 
		{
			// Set target frame rate (optional)
			Application.targetFrameRate = frameRate;
			_state = FrameRecorderState.IDLE;
			Init();
		}

		private void Init()
		{
			// Prepare textures and initial values
			_screenWidth = GetComponent<Camera>().pixelWidth;
			_screenHeight = GetComponent<Camera>().pixelHeight;
		
			tempRenderTexture = new RenderTexture(_screenWidth, _screenHeight, 0);
			tempTexture2D = new Texture2D(_screenWidth, _screenHeight, TextureFormat.RGB24, false);
			_frameQueue = new Queue<byte[]> ();

			_frameNumber = 0;
			_savingFrameNumber = 0;

			_captureFrameTime = 1.0f / (float)frameRate;
			_lastFrameTime = Time.time;
		}
		
		public void StartRecording()
		{
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
			
			_observer.NotifyStopped();
		}

		private void OnRenderImage(RenderTexture source, RenderTexture destination)
		{
			if (_state != FrameRecorderState.RECORDING)
			{
				Graphics.Blit (source, destination);
				return;
			}
			
			if (_frameNumber <= maxFrames)
			{
				// Check if render target size has changed, if so, terminate
				if(source.width != _screenWidth || source.height != _screenHeight)
				{
					_threadIsProcessing = false;
					this.enabled = false;
					throw new UnityException("FrameRecorder render target size has changed!");
				}

				// Calculate number of video frames to produce from this game frame
				// Generate 'padding' frames if desired framerate is higher than actual framerate
				float thisFrameTime = Time.time;
				int framesToCapture = ((int)(thisFrameTime / _captureFrameTime)) - ((int)(_lastFrameTime / _captureFrameTime));

				// Capture the frame
				if(framesToCapture > 0)
				{
					Graphics.Blit (source, tempRenderTexture);
				
					RenderTexture.active = tempRenderTexture;
					tempTexture2D.ReadPixels(new Rect(0, 0, Screen.width, Screen.height),0,0);
					RenderTexture.active = null;
				}

				// Add the required number of copies to the queue
				for(int i = 0; i < framesToCapture && _frameNumber <= maxFrames; ++i)
				{
					_frameQueue.Enqueue(tempTexture2D.GetRawTextureData());
					_frameNumber ++;
					//if(_frameNumber % frameRate == 0)
						//print ("Frame " + _frameNumber);
				}
			
				_lastFrameTime = thisFrameTime;
			}
			else //keep making screenshots until it reaches the max frame amount
			{
				_terminateThreadWhenDone = true;
				StopRecording();
			}
			Graphics.Blit (source, destination);
		}
	
		private void Encode()
		{
			print ("FRAMES RECORDER IO THREAD STARTED");
			while (_threadIsProcessing) 
			{
				if(_frameQueue.Count > 0)
				{
					using(MemoryStream memoryStream = new MemoryStream())
					{
						BitmapEncoder.WriteBitmap(
							memoryStream, _screenWidth, _screenHeight, _frameQueue.Dequeue());
						
						memoryStream.Close();
						_observer.NotifyFrameRecorded(memoryStream);
					}
					_savingFrameNumber ++;
					//print ("Saved " + _savingFrameNumber + " frames. " + _frameQueue.Count + " frames remaining.");
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
			print ("FRAMES RECORDER IO THREAD FINISHED");
		}
	}
}