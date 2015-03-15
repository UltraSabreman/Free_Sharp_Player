using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Free_Sharp_Player {
	using Timer = System.Timers.Timer;

	class StreamFrame {
		public StreamFrame Next { get; set; }
		public StreamFrame Prev { get; set; }

		private Mp3Frame theFrame;
		public Mp3Frame Frame {
			get {
				return theFrame;
			}
			set {
				theFrame = value;
				if (theFrame != null) {
					GetFrameLengthSec();
				}
			}
		}
		public bool SongChange { get; set; }
		public double FrameLengthSec { get; private set; }

		private void GetFrameLengthSec() {
			FrameLengthSec = ((double)Frame.SampleCount / (double)Frame.SampleRate);
		}

		public StreamFrame(Mp3Frame toAdd = null) {
			Next = null;
			Prev = null;
			Frame = toAdd;
			SongChange = false;
		}

	}

	class StreamQueue {
		//TODO: impliement queue seek
		//public int PosInQueue { get; set; } //change pos function
		public double MinBufferLengthSec { get; set; }
		public double MaxBufferLengthSec { get; set; }
		public double CurBufferLengthSec { get; set; }
		public int NumFramesToPlay { get; set; }
		public float Volume { get; set; }
		public PlaybackState StreamState { get; private set;}

		public delegate void TitleChange();
		public event TitleChange OnStreamTitleChange;

		private bool buffering;

		private StreamFrame head;
		private StreamFrame tail;
		private StreamFrame playHead;
		private IWavePlayer waveOut;

		private Timer playTimer;
		private VolumeWaveProvider16 volumeProvider;
		private BufferedWaveProvider bufferedWaveProvider;

		private int oldSampleRate;
		private Byte[] buffer; // needs to be big enough to hold a decompressed frame
		private IMp3FrameDecompressor decompressor;

		public StreamQueue(int min = 1, int max = 30, int num = 10) {
			playTimer = new Timer(250);
			playTimer.Elapsed += PlayFrame;
			playTimer.AutoReset = true;
			playTimer.Start();

			MinBufferLengthSec = min;
			MaxBufferLengthSec = max;
			NumFramesToPlay = num;
			Volume = 0.5f;

			buffering = false;

			oldSampleRate = -1;
			buffer = new Byte[16384 * 4];
			decompressor = null;
		}

		public void AddFrame(Mp3Frame frame, bool changed = false) {
			//Util.PrintLine(ConsoleColor.DarkCyan, "AddFrame");

			if (tail == null) {
				StreamFrame temp = new StreamFrame(frame);
				temp.SongChange = changed;

				playHead = head = tail = temp;
				temp.Next = temp.Prev = temp;
				CurBufferLengthSec += temp.FrameLengthSec;
			} else {
				if (CurBufferLengthSec >= MaxBufferLengthSec) {
					if (playHead == head)
						playHead = playHead.Next;
					head = head.Next;
					tail = tail.Next;
					tail.Frame = frame;
				} else {
					StreamFrame temp = new StreamFrame(frame);
					temp.SongChange = changed;

					tail.Next = temp;
					temp.Prev = tail;
					temp.Next = head;
					head.Prev = temp;

					tail = tail.Next;
					CurBufferLengthSec += temp.FrameLengthSec;
				}
			}
		}

		public bool IsBufferFull() {
			bool test = bufferedWaveProvider != null
				&& ((bufferedWaveProvider.BufferLength - bufferedWaveProvider.BufferedBytes)
				< bufferedWaveProvider.WaveFormat.AverageBytesPerSecond / 4);
			return test;
		}

		public void Play() {
			StreamState = PlaybackState.Playing;

			while (waveOut == null)
				Thread.Sleep(250);

			try {
				waveOut.Play();
			} catch (Exception) { }
		}

		public void Pause() {
			StreamState = PlaybackState.Paused;
			waveOut.Pause();
		}

		public void Stop() {
			if (StreamState != PlaybackState.Stopped) {

				StreamState = PlaybackState.Stopped;

				if (waveOut != null) {
					waveOut.Stop();
					waveOut.Dispose();
					waveOut = null;
				}

				if (bufferedWaveProvider != null)
					bufferedWaveProvider.ClearBuffer();
			}
		}


		private void PlayFrame(Object o, EventArgs e) {
			//Util.PrintLine(ConsoleColor.Green, "PlayTimer");
			if (playHead == null) return;
			if (CurBufferLengthSec < MinBufferLengthSec) {
				//TODO: fire event when buffering + when done.
				buffering = true;
				return;
			}
			if (StreamState != PlaybackState.Playing) return;

			if (buffering) {
				//TODO: fire buffering is done event
				buffering = false;
			}
			//Util.PrintLine(ConsoleColor.Yellow, "Playing");

			if (waveOut == null && bufferedWaveProvider != null) {
				waveOut = new WaveOut();

				volumeProvider = new VolumeWaveProvider16(bufferedWaveProvider);
				volumeProvider.Volume = Volume;

				waveOut.Init(volumeProvider);
			} else if (bufferedWaveProvider != null) 
				volumeProvider.Volume = Volume;
			
			double totalTime = 0;
			for (int i = 0; i < NumFramesToPlay; i++) {
				AddFrameToWaveBuffer(playHead.Frame);
				totalTime += playHead.FrameLengthSec * 1000;
				CurBufferLengthSec -= playHead.FrameLengthSec;

				if (playHead.SongChange) {
					if (OnStreamTitleChange != null)
						OnStreamTitleChange();
				}

				playHead = playHead.Next;
			}
			playTimer.Interval = totalTime;


		}

		private void AddFrameToWaveBuffer(Mp3Frame frame) {
			if (frame != null) {
				if (decompressor == null || (oldSampleRate != -1 && oldSampleRate != frame.SampleRate)) {
					// don't think these details matter too much - just help ACM select the right codec
					// however, the buffered provider doesn't know what sample rate it is working at
					// until we have a frame
					decompressor = CreateFrameDecompressor(frame);

					bufferedWaveProvider = new BufferedWaveProvider(decompressor.OutputFormat);
					bufferedWaveProvider.BufferDuration = TimeSpan.FromSeconds(MaxBufferLengthSec); // allow us to get well ahead of ourselves
					oldSampleRate = frame.SampleRate;
					//this.bufferedWaveProvider.BufferedDuration = 250;
				}


				int decompressed = decompressor.DecompressFrame(frame, buffer, 0);
				bufferedWaveProvider.AddSamples(buffer, 0, decompressed);
			}
		}

		private static IMp3FrameDecompressor CreateFrameDecompressor(Mp3Frame frame) {
			return new AcmMp3FrameDecompressor(new Mp3WaveFormat(frame.SampleRate, frame.ChannelMode == ChannelMode.Mono ? 1 : 2, frame.FrameLength, frame.BitRate));
		}

	/*private void waveLoader_Tick(object sender, EventArgs e) {
		if (PlaybackState != StreamingPlaybackState.Stopped) {
			if (waveOut == null && bufferedWaveProvider != null) {

				waveOut = new WaveOut();
				//waveOut.PlaybackStopped += OnPlaybackStopped;
				volumeProvider = new VolumeWaveProvider16(bufferedWaveProvider);
				volumeProvider.Volume = Volume;

				waveOut.Init(volumeProvider);
			} else if (bufferedWaveProvider != null) {
				volumeProvider.Volume = Volume;

				double bufferedSeconds = bufferedWaveProvider.BufferedDuration.TotalSeconds;
				BufferFillPercent = (int)((bufferedSeconds/ (double)bufferSizeSec) * 100);
				//DumpStreamStats(bufferedSeconds);
				// make it stutter less if we buffer up a decent amount before playing
				if (bufferedSeconds < 0.5 && PlaybackState == StreamingPlaybackState.Playing && !EndOfStream)
					Buffer();
				else if (bufferedSeconds > 4 && PlaybackState == StreamingPlaybackState.Buffering)
					Play();
				else if (EndOfStream && bufferedSeconds == 0)
					Stop();
					
			}

		}
	}*/

	}
}
