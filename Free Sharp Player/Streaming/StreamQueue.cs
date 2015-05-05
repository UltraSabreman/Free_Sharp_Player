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

		public void Delete() {
			Next = null;
			Prev = null;
			Frame = null;
			SongChange = false;
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
		public double CurTotalBufferLengthSec { get; set; }
		public double NumSecToPlay { get; set; }
		public float Volume { get; set; }
		public bool IsPlaying { get; private set;}


		public delegate void SongChange(DateTime? newPlayDate);
		public event SongChange OnStreamSongChange;

		public delegate void Buffering(bool isBuffering);
		public event Buffering OnBufferingStateChange;

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

		private DateTime startTime;


		public StreamQueue(int min = 5, int max = 10, double sec = 3) {
			playTimer = new Timer(250);
			playTimer.Elapsed += PlayFrame;
			//playTimer.AutoReset = true;
			playTimer.Start();

			MinBufferLengthSec = min;
			MaxBufferLengthSec = max;
			NumSecToPlay = sec;
			Volume = 0.5f;

			buffering = false;

			oldSampleRate = -1;
			buffer = new Byte[16384 * 4];
			decompressor = null;

			startTime = DateTime.Now;
		}

		public void AddFrame(Mp3Frame frame, bool changed = false) {
			lock (addLock) {
				if (tail == null) {
					StreamFrame temp = new StreamFrame(frame);
					temp.SongChange = changed;

					playHead = head = tail = temp;
					CurBufferLengthSec += temp.FrameLengthSec;
					CurTotalBufferLengthSec += temp.FrameLengthSec;
				} else {
					StreamFrame temp = new StreamFrame(frame);
					temp.SongChange = changed;

					tail.Next = temp;
					temp.Prev = tail;
					tail = tail.Next;

					CurTotalBufferLengthSec += temp.FrameLengthSec;
					CurBufferLengthSec += temp.FrameLengthSec;

					if (CurTotalBufferLengthSec >= MaxBufferLengthSec) {
						if (playHead == head)
							playHead = playHead.Next;
						
						CurTotalBufferLengthSec -= head.FrameLengthSec;
						head = head.Next;
						head.Prev.Delete();
						head.Prev = null;
					}
				}

			}
		}

		public Mp3Frame GetFrame(ref bool doEvent) {
			CurBufferLengthSec -= playHead.FrameLengthSec;

			if (playHead.Next == null) return null;
	
			//We dont want to reset this to false if it's already true.
			doEvent = playHead.SongChange ? true : doEvent;
			Mp3Frame ret = playHead.Frame;
			playHead = playHead.Next;

			return ret;
		}

		public bool IsBufferFull() {
			return CurBufferLengthSec >= (MaxBufferLengthSec - 2);
		}

		public void Play() {
			if (IsPlaying) return;
			IsPlaying = true;
		}

		public void Stop() {
			if (!IsPlaying) return;
			IsPlaying = false;

			if (waveOut != null) {
				waveOut.Stop();
				waveOut.Dispose();
				waveOut = null;
			}

			if (bufferedWaveProvider != null) {
				bufferedWaveProvider.ClearBuffer();
				bufferedWaveProvider = null;
			}

			if (decompressor != null) {
				decompressor.Dispose();
				decompressor = null;
			}

			//TODO: actually delete the list?
			tail = playHead = head = null;
		}
		private double oldBuffSecs = 0;
		private double oldWaveSecs = 0;

		private void PlayFrame(Object o, EventArgs e) {
			lock (playLock) {
				//try {
					//Check if queue has shit in it.
					if (playHead == null) return;
					//Dont play if we're not playing.
					if (!IsPlaying) return;

					//Don't play if we're buffering.
					if (CurBufferLengthSec < MinBufferLengthSec) {
						//TODO: fire event buffering
						buffering = true;
						if (OnBufferingStateChange != null)
							OnBufferingStateChange(buffering);
						return;
					}

					//We're now done buffering
					if (buffering) {
						buffering = false;
						if (OnBufferingStateChange != null)
							OnBufferingStateChange(buffering);
					}

					//Initilize wave provider if its null
					if (waveOut == null && bufferedWaveProvider != null) {
						waveOut = new WaveOut();

						volumeProvider = new VolumeWaveProvider16(bufferedWaveProvider);
						volumeProvider.Volume = Volume;

						waveOut.Init(volumeProvider);
					} else if (bufferedWaveProvider != null)
						volumeProvider.Volume = Volume; //set volume
					//TODO: set volume dinamically.

					bool doEvent = false;
					double totalTime = 0;
					int frames = 0;

					//Get time in wave buffer and diffrence between that and what we want
					double inWavBuff = (bufferedWaveProvider == null ? 0 : bufferedWaveProvider.BufferedDuration.TotalSeconds);
					double diff = NumSecToPlay - inWavBuff;

					//If the player is "catching up" to our limit
					//We load an extra second to give it a break.
					if (diff < 0.5)
						diff += 1;
					else if (diff > (NumSecToPlay - 0.5))
						diff -= 1;

					//Load frames into provider
					while (bufferedWaveProvider == null || bufferedWaveProvider.BufferedDuration.TotalSeconds < NumSecToPlay) {
						totalTime += playHead.FrameLengthSec;
						Mp3Frame temp = GetFrame(ref doEvent);
						AddFrameToWaveBuffer(temp);
						frames++;
					}

					if (waveOut != null && waveOut.PlaybackState != PlaybackState.Playing)
						waveOut.Play();

					if (doEvent) {
						startTime = DateTime.Now;
						if (OnStreamSongChange != null)
							OnStreamSongChange(DateTime.Now);
					}

					new Thread((obj) => {
						Console.Clear();
						Util.PrintLine("Frames Played: ", ConsoleColor.Cyan, frames);
						Util.PrintLine("Time Diffrence: ", ConsoleColor.White, String.Format("{0:0.000}", diff));
						Util.PrintLine("Time Loaded: ", ConsoleColor.DarkYellow, String.Format("{0:0.000}", totalTime));
						double BuffDelta = CurBufferLengthSec - oldBuffSecs;
						double WaveDelta = bufferedWaveProvider.BufferedDuration.TotalSeconds - oldWaveSecs;
						Util.PrintLine("Time In Buffer: ", ConsoleColor.DarkYellow, CurTotalBufferLengthSec);
						Util.PrintLine("Time In Stream Buffer: ", ConsoleColor.DarkYellow, String.Format("{0:0.000}", CurBufferLengthSec), ConsoleColor.White, " Δ", ": ", (BuffDelta < 0 ? ConsoleColor.Red : ConsoleColor.Green), String.Format("{0:0.000}", BuffDelta));
						Util.PrintLine("Time In Wave Buffer: ", ConsoleColor.DarkYellow, String.Format("{0:0.000}", bufferedWaveProvider.BufferedDuration.TotalSeconds), ConsoleColor.White, " Δ", ": ", (WaveDelta < 0 ? ConsoleColor.Red : ConsoleColor.Green), String.Format("{0:0.000}", WaveDelta));
						Util.PrintLine("Time Since songChange: ", ConsoleColor.Yellow, startTime - DateTime.Now);
						oldBuffSecs = CurBufferLengthSec;
						oldWaveSecs = bufferedWaveProvider.BufferedDuration.TotalSeconds;
					}).Start();

					playTimer.Interval = diff * 1000;
					playTimer.Start();

				/*} catch (Exception ex) {
					Util.DumpException(ex);
					throw ex;
				}*/
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
