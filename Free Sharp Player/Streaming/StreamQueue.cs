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

	public class StreamFrame {
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

	public class StreamQueue {
		private Object addLock = new Object();
		private Object playLock = new Object();

		//TODO: impliement queue seek
		//public int PosInQueue { get; set; } //change pos function
		public double MinBufferLengthSec { get; set; }
		public double MaxBufferLengthSec { get; set; }
		public double CurBufferLengthSec { get; set; }
		public double NumSecToPlay { get; set; }
		public float Volume { get; set; }
		public bool IsPlaying { get; private set;}


		public delegate void SongChange(DateTime? newPlayDate);
		public event SongChange OnStreamSongChange;

		private bool buffering;

		private StreamFrame head;
		private StreamFrame tail;
		private StreamFrame playHead;
		private Timer playTimer;

		private IWavePlayer waveOut;
		private VolumeWaveProvider16 volumeProvider;
		private BufferedWaveProvider bufferedWaveProvider;

		private int oldSampleRate;
		private Byte[] buffer; // needs to be big enough to hold a decompressed frame
		private IMp3FrameDecompressor decompressor;


		public StreamQueue(int min = 1, int max = 30, double sec = 2) {
			playTimer = new Timer(250);
			playTimer.Elapsed += PlayFrame;
			playTimer.AutoReset = true;
			playTimer.Start();

			MinBufferLengthSec = min;
			MaxBufferLengthSec = max;
			NumSecToPlay = sec;
			Volume = 0.5f;

			buffering = false;

			oldSampleRate = -1;
			buffer = new Byte[16384 * 4];
			decompressor = null;
		}

		public void AddFrame(Mp3Frame frame, bool changed = false) {
			lock (addLock) {
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
		}

		public bool IsBufferFull() {
			return CurBufferLengthSec >= (MaxBufferLengthSec - 2);
		}

		public void Play() {
			if (IsPlaying) return;
			IsPlaying = true;

			//TODO: fire loading event
			new Thread(() => {
				//TODO: deal with this:
				while (waveOut == null)
					Thread.Sleep(250);

				try {
					waveOut.Play();
				} catch (Exception) { }
				//TODO: fire loaded event
			}).Start();
		}

		public void Stop() {
			if (!IsPlaying) return;
			IsPlaying = false;

			if (waveOut != null) {
				waveOut.Stop();
				waveOut.Dispose();
				waveOut = null;
			}

			if (bufferedWaveProvider != null)
				bufferedWaveProvider.ClearBuffer();

			//TODO: actually delete the list?
			tail = playHead = head = null;
		}


		private void PlayFrame(Object o, EventArgs e) {
			lock (playLock) {
				//Util.PrintLine(ConsoleColor.Green, "PlayTimer");
				if (playHead == null) return;
				if (CurBufferLengthSec < MinBufferLengthSec) {
					//TODO: fire event buffering
					buffering = true;
					return;
				}
				if (!IsPlaying) return;

				if (buffering) {
					//TODO: fire event buffering done
					buffering = false;
				}

				if (bufferedWaveProvider != null) {
					waveOut = new WaveOut();

					volumeProvider = new VolumeWaveProvider16(bufferedWaveProvider);
					volumeProvider.Volume = Volume;

					waveOut.Init(volumeProvider);
				} else if (bufferedWaveProvider != null)
					volumeProvider.Volume = Volume;

				bool doEvent = false;
				double totalTime = 0;
				//int frames = 0;
				while (totalTime < NumSecToPlay) {
					AddFrameToWaveBuffer(playHead.Frame);
					totalTime += playHead.FrameLengthSec;
					CurBufferLengthSec -= playHead.FrameLengthSec;
					//frames ++;
					if (playHead.SongChange) doEvent = true;

					playHead = playHead.Next;
				}

				if (doEvent && OnStreamSongChange != null) {
					OnStreamSongChange(DateTime.Now);
				}

				/*new Thread((obj) => {
					Console.Clear();
					Util.PrintLine("Frames Played: ", ConsoleColor.Cyan, frames);
					Util.PrintLine("Time Loaded: ", ConsoleColor.Green, totalTime);
					Util.PrintLine("Time In Buffer: ", ConsoleColor.DarkYellow, CurBufferLengthSec);
					Util.PrintLine("Time In WaveBuffer: ", ConsoleColor.Red, bufferedWaveProvider.BufferedDuration.TotalSeconds);
					Util.PrintLine("Time Since songChange: ", ConsoleColor.Yellow, startTime - DateTime.Now);
				}).Start();*/

				playTimer.Interval = totalTime * 1000;

			}
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

				try {
					int decompressed = decompressor.DecompressFrame(frame, buffer, 0);
					bufferedWaveProvider.AddSamples(buffer, 0, decompressed);
				} catch (NAudio.MmException e) {
					//TODO: Handle stream exception
					Util.DumpException(e);
				}
			}
		}

		private static IMp3FrameDecompressor CreateFrameDecompressor(Mp3Frame frame) {
			return new AcmMp3FrameDecompressor(new Mp3WaveFormat(frame.SampleRate, frame.ChannelMode == ChannelMode.Mono ? 1 : 2, frame.FrameLength, frame.BitRate));
		}
	}
}
