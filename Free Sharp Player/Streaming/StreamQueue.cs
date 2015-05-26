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


	public class EventTuple {
		public EventType Event { get; set; }
		public double EventQueuePosition { get; set; }
		public Track CurrentSong { get; set; }
		public getRadioInfo RadioInfo { get; set; }
	}


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
		public EventTuple Event { get; set; }

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
			Event = null;
		}

		public StreamFrame(Mp3Frame toAdd = null) {
			Next = null;
			Prev = null;
			Event = null;
			Frame = toAdd;
		}

	}

	public class QueueSettingsTuple { 
		public double MaxBufferedTime { get; set; }	//Minimum time buffered to play (before stream starts buffering again)
		public double MinBufferedTime { get; set; }	//Minimum time buffered to play (before stream starts buffering again)
		public double BufferedTime { get; set; }	//Time currently buffered to play

		public double MaxTimeInQueue { get; set; }	//Maximum TOTAL time that can be stored in the queue.
		public double TotalTimeInQueue { get; set; } //Total size of the queue currently

		public double NumSecToPlay { get; set; }

		public void Copy(QueueSettingsTuple src) {
			MaxBufferedTime = src.MaxBufferedTime;
			MinBufferedTime = src.MinBufferedTime;
			BufferedTime = src.BufferedTime;
			MaxTimeInQueue = src.MaxTimeInQueue;
			TotalTimeInQueue = src.TotalTimeInQueue;
			NumSecToPlay = src.NumSecToPlay;
		}
	}

	public class StreamQueue {
		private Object addLock = new Object();
		private Object playLock = new Object();
		private Object printLock = new Object();


		private QueueSettingsTuple Settings { get; set; }
		private float volume = 0.5f;
		public float Volume { 
			get {
				return volume;
			}
			set {
				if (volumeProvider != null)
					volumeProvider.Volume = value;
				volume = value;
			}
		}


		public bool IsPlaying { get; private set;}


		#region events
		public delegate void EventTriggered(EventTuple e);
		public event EventTriggered OnEventTrigger;

		public delegate void Buffering(bool isBuffering);
		public event Buffering OnBufferingStateChange;

		public delegate void QueueTick(QueueSettingsTuple theStuff);
		public event QueueTick OnQueueTick;
		#endregion events

		private bool buffering;

		private StreamFrame head;
		private StreamFrame tail;
		private StreamFrame playHead;

		private List<EventTuple> allEvents = new List<EventTuple>();
		
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

		public StreamQueue(QueueSettingsTuple newSettings){ 
			playTimer = new Timer(250);
			playTimer.Elapsed += PlayFrame;

			Settings = newSettings;

			buffering = false;

			oldSampleRate = -1;
			buffer = new Byte[16384 * 4];
			decompressor = null;

			frames = 0;
			diff = 0;
			totalTime = 0;

			playTimer.Start();

			//StreamInfo();
		}


		public void AddFrame(Mp3Frame frame, EventTuple theEvent = null) {
			lock (addLock) {
				StreamFrame temp = new StreamFrame(frame);

				if (theEvent != null) {
					theEvent.EventQueuePosition = Settings.TotalTimeInQueue;
					allEvents.Add(theEvent);
				}

				temp.Event = theEvent;

				if (tail == null) {
					playHead = head = tail = temp;
					Settings.BufferedTime += temp.FrameLengthSec;
					Settings.TotalTimeInQueue += temp.FrameLengthSec;
				} else {
					tail.Next = temp;
					temp.Prev = tail;
					tail = tail.Next;

					Settings.TotalTimeInQueue += temp.FrameLengthSec;
					Settings.BufferedTime += temp.FrameLengthSec;

					if (Settings.TotalTimeInQueue >= Settings.MaxTimeInQueue) {
						if (playHead == head) {
							playHead = playHead.Next;
							Settings.BufferedTime -= head.FrameLengthSec;
						}
						
						Settings.TotalTimeInQueue -= head.FrameLengthSec;

						allEvents.RemoveAt(0);
						foreach (EventTuple e in allEvents)
							e.EventQueuePosition -= head.FrameLengthSec;

						head = head.Next;
						head.Prev.Delete();
						head.Prev = null;
					}
				}

			}
		}

		public List<EventTuple> GetAllEvents() {
			lock (addLock) {
				return new List<EventTuple>(allEvents);
			}
		}

		//TODO: remove code duplication
		public void Seek(double secToSeek) {
			lock (playLock) {
				lock (addLock) {
					if (secToSeek < 0) {
						double destTime = Settings.BufferedTime - secToSeek;

						while (Settings.BufferedTime <= destTime && playHead.Prev != null) {
							Settings.BufferedTime += playHead.FrameLengthSec;

							playHead = playHead.Prev;
						}

						Settings.MaxBufferedTime -= secToSeek;
					} else {
						double destTime = Settings.BufferedTime - secToSeek;

						while (Settings.BufferedTime >= destTime && playHead.Next != null) {
							Settings.BufferedTime -= playHead.FrameLengthSec;

							playHead = playHead.Next;
						}



						Settings.MaxBufferedTime += secToSeek;

					}

					if (playHead.Event != null) {
						fireEvent(playHead.Event);
					}
					
				}
			}
		}

		public bool IsBufferFull() {
			return Settings.BufferedTime >= Settings.MaxBufferedTime;
		}

		public void PurgeQueue() {
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
				Settings.BufferedTime = 0;
				Settings.TotalTimeInQueue = 0;

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


		private Mp3Frame GetFrame(ref EventTuple doEvent) {
			if (playHead.Event != null && playHead.Event.Event != EventType.None)
				doEvent = playHead.Event;

			Mp3Frame ret = playHead.Frame;
			Settings.BufferedTime -= playHead.FrameLengthSec;

			if (playHead.Next != null)
				playHead.Next.Event.EventQueuePosition -= playHead.FrameLengthSec;

			playHead = playHead.Next;

			return ret;
		}

		private void PlayFrame(Object o, EventArgs e) {
			lock (playLock) {
				//TODO: is this the right place for the tick event?
				if (OnQueueTick != null)
					OnQueueTick(Settings);

				try {
					//Check if queue has shit in it.
					if (playHead == null) return;
					//Dont play if we're not playing.
					if (!IsPlaying) return;

					//Don't play if we're buffering.
					if (Settings.BufferedTime < Settings.MinBufferedTime) {
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
					double diff = Settings.NumSecToPlay - inWavBuff;

					if (diff <= 0)
						diff = 0.1;

					playTimer.Interval = diff * 1000;

					EventTuple doEvent = null;
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

					if (doEvent != null)
						fireEvent(doEvent);

					playTimer.Start();

				} catch (NullReferenceException ex) { //happens when restarting the stream.
					Util.DumpException(ex);
				}
			}
		}

		private void fireEvent(EventTuple tup) {

			if (OnEventTrigger != null)
				OnEventTrigger(tup);
		}

		private void AddFrameToWaveBuffer(Mp3Frame frame) {
			if (frame != null) {
				if (decompressor == null || (oldSampleRate != -1 && oldSampleRate != frame.SampleRate)) {
					// don't think these details matter too much - just help ACM select the right codec
					// however, the buffered provider doesn't know what sample rate it is working at
					// until we have a frame
					decompressor = CreateFrameDecompressor(frame);

					bufferedWaveProvider = new BufferedWaveProvider(decompressor.OutputFormat);
					bufferedWaveProvider.BufferDuration = TimeSpan.FromSeconds(Settings.MaxTimeInQueue); // allow us to get well ahead of ourselves
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

		#region CLI Output

		public void Draw() {
			var white = ConsoleColor.White;
			var green = ConsoleColor.Green;
			var black = ConsoleColor.Black;
			var yellow = ConsoleColor.Yellow;
			char b = '█';

			List<EventTuple> thing = GetAllEvents();

			double percentfull = (Settings.TotalTimeInQueue / Settings.MaxTimeInQueue) * 100;
			double percentBuff = (Settings.BufferedTime / Settings.TotalTimeInQueue);

			Util.Print(white, "[");
			bool changed = false;
			for (int i = 0; i < 100; i++) {
				if (i < percentfull) {
					double buff = (1 - percentBuff) * percentfull;
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

			foreach (EventTuple e in thing) {
				int pos = (int)Math.Max(Math.Round((e.EventQueuePosition / Settings.TotalTimeInQueue) * ((Settings.TotalTimeInQueue / Settings.MaxTimeInQueue) * 100)), 0);
				Console.CursorLeft = pos;
				if (e.Event == EventType.SongChange)
					Util.Print(ConsoleColor.Cyan, "S");
				else if (e.Event == EventType.Disconnect)
					Util.Print(ConsoleColor.Red, "D");
			}
		}
		private void StreamInfo() {
			var test = new System.Timers.Timer();
			test.Interval = 100;
			test.AutoReset = true;
			test.Enabled = false;

			test.Elapsed += (o, e) => {
				lock (printLock) {
					Console.Clear();
					Util.PrintLine("Frames Played: ", ConsoleColor.Cyan, frames);
					Util.PrintLine("Time Diffrence: ", ConsoleColor.White, String.Format("{0:0.000}", diff));
					Util.PrintLine("Time Loaded: ", ConsoleColor.DarkYellow, String.Format("{0:0.000}", totalTime));
					double BuffDelta = Settings.BufferedTime - oldBuffSecs;
					double totes = bufferedWaveProvider != null ? bufferedWaveProvider.BufferedDuration.TotalSeconds : -1;
					double WaveDelta = totes - oldWaveSecs;
					Util.PrintLine("Time In Buffer: ", ConsoleColor.DarkYellow, Settings.TotalTimeInQueue);
					Util.PrintLine("Time In Stream Buffer: ", ConsoleColor.DarkYellow, String.Format("{0:0.000}", Settings.BufferedTime), ConsoleColor.White, " Δ", ": ", (BuffDelta < 0 ? ConsoleColor.Red : ConsoleColor.Green), String.Format("{0:0.000}", BuffDelta));
					Util.PrintLine("Time In Wave Buffer: ", ConsoleColor.DarkYellow, String.Format("{0:0.000}", totes), ConsoleColor.White, " Δ", ": ", (WaveDelta < 0 ? ConsoleColor.Red : ConsoleColor.Green), String.Format("{0:0.000}", WaveDelta));
					Util.PrintLine("Time Since songChange: ", ConsoleColor.Yellow, startTime - DateTime.Now);
					Draw();
					oldBuffSecs = Settings.BufferedTime;
					oldWaveSecs = totes;
				}
			};
			//test.Start();
		}

		#endregion CLI Output

	}
}
