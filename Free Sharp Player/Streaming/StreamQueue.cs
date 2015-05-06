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

	public enum EventType {None, SongChange, Disconnect}

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
		public EventType Event { get; set; }
		public double FrameLengthSec { get; private set; }

		private void GetFrameLengthSec() {
			if (Frame != null)
				FrameLengthSec = ((double)Frame.SampleCount / (double)Frame.SampleRate);
			else 
				FrameLengthSec = 0;
		}

		public void Delete() {
			Next = null;
			Prev = null;
			Frame = null;
			Event = EventType.None;
		}

		public StreamFrame(Mp3Frame toAdd = null) {
			Next = null;
			Prev = null;
			Frame = toAdd;
			if (Frame == null)
				Event = EventType.Disconnect;
			else
				Event = EventType.None;
		}

	}

	public class EventPair {
		public EventType Event { get; set; }
		public double EventQueuePosition { get; set; }
	}

	public class StreamQueue {
		private Object addLock = new Object();
		private Object playLock = new Object();
		private Object printLock = new Object();

		//TODO: impliement queue seek
		//public int PosInQueue { get; set; } //change pos function

		public double MaxBufferdTime { get; set; }	//Minimum time buffered to play (before stream starts buffering again)
		public double MinBufferdTime { get; set; }	//Minimum time buffered to play (before stream starts buffering again)
		public double MaxTineInQueue { get; set; }	//Maximum TOTAL time that can be stored in the queue.
		public double BufferedTime { get; set; }	//Time currently buffered to play
		public double TotalTimeInQueue { get; set; }//Total size of the queue currently

		public double NumSecToPlay { get; set; }
		public float Volume { get; set; }
		public bool IsPlaying { get; private set;}


		public delegate void EventTriggered(EventType e);
		public event EventTriggered OnEventTrigger;

		public delegate void Buffering(bool isBuffering);
		public event Buffering OnBufferingStateChange;

		public delegate void Disconnect();
		public event Disconnect OnStreamDisconnect;

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

		private double oldBuffSecs = 0;
		private double oldWaveSecs = 0;
		private int frames;
		private double diff;
		private double totalTime;

		public StreamQueue(int maxBuff = 30, int maxQueue = 300, double minBuffer = 3, double insertSec = 0.5) {
			playTimer = new Timer(250);
			playTimer.Elapsed += PlayFrame;

			MinBufferdTime = minBuffer;
			MaxBufferdTime = maxBuff;
			MaxTineInQueue = maxQueue;
			NumSecToPlay = insertSec;
			Volume = 0.5f;

			buffering = false;

			oldSampleRate = -1;
			buffer = new Byte[16384 * 4];
			decompressor = null;

			frames = 0;
			diff = 0;
			totalTime = 0;

			playTimer.Start();

			StreamInfo();
		}

		public void AddFrame(Mp3Frame frame, bool changed = false) {
			lock (addLock) {
				if (tail == null) {
					StreamFrame temp = new StreamFrame(frame);
					if (changed) temp.Event = EventType.SongChange;

					playHead = head = tail = temp;
					BufferedTime += temp.FrameLengthSec;
					TotalTimeInQueue += temp.FrameLengthSec;
				} else {
					StreamFrame temp = new StreamFrame(frame);
					if (changed) temp.Event = EventType.SongChange;

					tail.Next = temp;
					temp.Prev = tail;
					tail = tail.Next;

					TotalTimeInQueue += temp.FrameLengthSec;
					BufferedTime += temp.FrameLengthSec;

					if (TotalTimeInQueue >= MaxTineInQueue) {
						if (playHead == head) {
							playHead = playHead.Next;
							BufferedTime -= head.FrameLengthSec;
						}
						
						TotalTimeInQueue -= head.FrameLengthSec;
						head = head.Next;
						head.Prev.Delete();
						head.Prev = null;
					}
				}

			}
		}

		public List<EventPair> GetAllEvents() {
			lock (addLock) {
				List<EventPair> nums = new List<EventPair>();

				StreamFrame cur = head;
				double time = 0;
				while (cur != null) {
					if (cur.Event != EventType.None)
						nums.Add(new EventPair {Event = cur.Event, EventQueuePosition = time});

					time += cur.FrameLengthSec;
					cur = cur.Next;
				}

				return nums;
			}
		}

		public void Seek(double secToSeek) {
			lock (playLock) {
				lock (addLock) {
					if (secToSeek < 0) {
						double destTime = BufferedTime - secToSeek;

						while (BufferedTime <= destTime && playHead.Prev != null) {
							BufferedTime += playHead.FrameLengthSec;

							playHead = playHead.Prev;
						}
					} else {
						double destTime = BufferedTime + secToSeek;
						while (BufferedTime >= destTime && playHead.Next != null) {
							BufferedTime -= playHead.FrameLengthSec;

							playHead = playHead.Next;
						}
					}
					
				}
			}
		}

		public bool IsBufferFull() {
			return BufferedTime >= MaxBufferdTime;
		}

		public void Draw() {
			var white = ConsoleColor.White;
			var green = ConsoleColor.Green;
			var black = ConsoleColor.Black;
			var yellow = ConsoleColor.Yellow;
			char b = '█';

			List<EventPair> thing = GetAllEvents();

			double percentfull = (TotalTimeInQueue / MaxTineInQueue) * 100;
			double percentBuff = (BufferedTime / TotalTimeInQueue);

			Util.Print(white, "[");
			bool changed = false;
			for (int i = 0; i < 100; i++) {
				if (i < percentfull) {
					double buff = (1-percentBuff) * percentfull;
					if (i < buff)
						Util.Print(yellow, b);
					else {
						if (!changed) {
							changed = true;
							Console.CursorTop += 1;
							Console.CursorLeft -= 1;
							Util.Print(white, "P");
							Console.CursorTop -= 1;
						}
						Util.Print(green, b);
					}
				} else
					Util.Print(black, b);
			}
			Util.PrintLine(white, "]");

			foreach (EventPair e in thing) {
				int pos =  (int)Math.Max(Math.Round((e.EventQueuePosition / TotalTimeInQueue) * ((TotalTimeInQueue / MaxTineInQueue) * 100)), 0);
				Console.CursorLeft = pos;
				if (e.Event == EventType.SongChange)
					Util.Print(ConsoleColor.Cyan, "S");
				else if (e.Event == EventType.Disconnect)
					Util.Print(ConsoleColor.Red, "D");
			}
		}

		public void PurgeQueue() {
			//lock (addLock) {

				while (head != null) {
					var temp = head.Next;
					if (temp != null)
						temp.Prev = null;
					if (head != null)
						head.Delete();
					head = temp;
				}
				tail = null;
				playHead = null;
			//}
		}

		public Mp3Frame GetFrame(ref EventType doEvent) {
	
			//We dont want to reset this to false if it's already true.
			doEvent = playHead.Event;
			Mp3Frame ret = playHead.Frame;

			BufferedTime -= playHead.FrameLengthSec;
			playHead = playHead.Next;

			return ret;
		}

		public void Play() {
			if (IsPlaying) return;
			startTime = DateTime.Now;
			IsPlaying = true;
		}

		public void Stop() {
			if (!IsPlaying) return;
			IsPlaying = false;

			PurgeQueue();

			lock (playLock) {

				BufferedTime = 0;
				TotalTimeInQueue = 0;

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
			}
		}


		private void PlayFrame(Object o, EventArgs e) {
			lock (playLock) {
				try {
					//Check if queue has shit in it.
					if (playHead == null) return;
					//Dont play if we're not playing.
					if (!IsPlaying) return;

					//Don't play if we're buffering.
					if (BufferedTime < MinBufferdTime) {
						if (!buffering && OnBufferingStateChange != null)
							OnBufferingStateChange(buffering);
						buffering = true;

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


					//Get time in wave buffer and diffrence between that and what we want
					double inWavBuff = (bufferedWaveProvider == null ? 0 : bufferedWaveProvider.BufferedDuration.TotalSeconds);
					double diff = NumSecToPlay - inWavBuff;

					if (diff <= 0)
						diff = 0.1;

					playTimer.Interval = diff * 1000;

					EventType doEvent = EventType.None;
					totalTime = 0;
					double oldTotalTime = -1; //This prevents a deadlock
					frames = 0;

					//Load frames into provider
					//bufferedWaveProvider.BufferedDuration.TotalSeconds
					while (diff > 0 && oldTotalTime != totalTime) {
						oldTotalTime = totalTime;
						totalTime += playHead.FrameLengthSec;
						diff -= playHead.FrameLengthSec;
						Mp3Frame temp = GetFrame(ref doEvent);
						AddFrameToWaveBuffer(temp);
						frames++;
					}

					if (waveOut != null && waveOut.PlaybackState != PlaybackState.Playing)
						waveOut.Play();

					if (doEvent != EventType.None) {
						if (doEvent == EventType.SongChange)
							startTime = DateTime.Now;
						if (OnEventTrigger != null)
							OnEventTrigger(doEvent);
					}

					playTimer.Start();

				} catch (NullReferenceException ex) { //happens when restarting the stream.
					Util.DumpException(ex);
				}
			}
		}

		private void StreamInfo() {
			var test = new System.Timers.Timer();
			test.Interval = 100;
			test.AutoReset = true;
			test.Enabled = true;

			test.Elapsed += (o, e) => {
				lock (printLock) {
					Console.Clear();
					Util.PrintLine("Frames Played: ", ConsoleColor.Cyan, frames);
					Util.PrintLine("Time Diffrence: ", ConsoleColor.White, String.Format("{0:0.000}", diff));
					Util.PrintLine("Time Loaded: ", ConsoleColor.DarkYellow, String.Format("{0:0.000}", totalTime));
					double BuffDelta = BufferedTime - oldBuffSecs;
					double totes = bufferedWaveProvider!= null ? bufferedWaveProvider.BufferedDuration.TotalSeconds : -1;
					double WaveDelta = totes - oldWaveSecs;
					Util.PrintLine("Time In Buffer: ", ConsoleColor.DarkYellow, TotalTimeInQueue);
					Util.PrintLine("Time In Stream Buffer: ", ConsoleColor.DarkYellow, String.Format("{0:0.000}", BufferedTime), ConsoleColor.White, " Δ", ": ", (BuffDelta < 0 ? ConsoleColor.Red : ConsoleColor.Green), String.Format("{0:0.000}", BuffDelta));
					Util.PrintLine("Time In Wave Buffer: ", ConsoleColor.DarkYellow, String.Format("{0:0.000}", totes), ConsoleColor.White, " Δ", ": ", (WaveDelta < 0 ? ConsoleColor.Red : ConsoleColor.Green), String.Format("{0:0.000}", WaveDelta));
					Util.PrintLine("Time Since songChange: ", ConsoleColor.Yellow, startTime - DateTime.Now);
					Draw();
					oldBuffSecs = BufferedTime;
					oldWaveSecs = totes;
				}
			};
			test.Start();
		}

		private void AddFrameToWaveBuffer(Mp3Frame frame) {
			if (frame != null) {
				if (decompressor == null || (oldSampleRate != -1 && oldSampleRate != frame.SampleRate)) {
					// don't think these details matter too much - just help ACM select the right codec
					// however, the buffered provider doesn't know what sample rate it is working at
					// until we have a frame
					decompressor = CreateFrameDecompressor(frame);

					bufferedWaveProvider = new BufferedWaveProvider(decompressor.OutputFormat);
					bufferedWaveProvider.BufferDuration = TimeSpan.FromSeconds(MaxTineInQueue); // allow us to get well ahead of ourselves
					oldSampleRate = frame.SampleRate;
					//this.bufferedWaveProvider.BufferedDuration = 250;
				}

				try {
					int decompressed = decompressor.DecompressFrame(frame, buffer, 0);
					bufferedWaveProvider.AddSamples(buffer, 0, decompressed);
				}  catch (NAudio.MmException e) {
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
