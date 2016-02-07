using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Free_Sharp_Player {
    using Timer = System.Timers.Timer;

    public enum EventType { None, SongChange, StateChange }
    public enum StreamState { Buffering, Stopped, Paused, Playing }


	public class MusicStream {
        #region Public Properties
        public double MaxBufferdTime { get; set; }	    //Maxnimum time to buffer before we stop downloading
		public double MinBufferdTime { get; set; }	    //Minimum time buffered to play (before stream starts buffering again)
		public double MaxTimeInQueue { get; set; }	    //Maximum TOTAL time that can be stored in the queue.
		public double BufferedTime { get; set; }	    //Time currently buffered to play
		public double TotalTimeInQueue { get; set; }    //Total size of the queue currently
		public double NumSecToPlay { get; set; }
        public StreamState State { get; private set; }

        public float Volume { get {
                return volumeProvider.Volume;
            }
            set {
                if (volumeProvider != null)
                    volumeProvider.Volume = value;
            }
        }

		public delegate void EventTriggered(EventTuple e);
		public event EventTriggered OnStreamEvent;
        #endregion

        #region Private Variables
        private Object addLock = new Object();
        private Object playLock = new Object();
        private Object printLock = new Object();

        private String address;
        private bool endOfStream = false;
        private Thread streamThread;


        private StreamFrame head;
		private StreamFrame tail;
		private StreamFrame playHead;
		private Timer playTimer;

		private IWavePlayer waveOut;
		private VolumeWaveProvider16 volumeProvider;
		private BufferedWaveProvider bufferedWaveProvider;

		private Byte[] buffer; // needs to be big enough to hold a decompressed frame
		private IMp3FrameDecompressor decompressor;

        private int oldSampleRate;
        private float initVolume;

        private bool isInitilized = false;

        /*
        private double oldBuffSecs;
        private double diff;
		private double oldWaveSecs;
		private int frames;
		private double totalTime;
        */

        #endregion

        public MusicStream(String streamAddress, int maxBuff = 30, int maxQueue = 300, double minBuffer = 3, double insertSec = 0.5, float initVolume = 0.5f) {
            address = streamAddress;

			MinBufferdTime = minBuffer;
			MaxBufferdTime = maxBuff;
			MaxTimeInQueue = maxQueue;
			NumSecToPlay = insertSec;
			Volume = 0.5f;

            State = StreamState.Stopped;

			oldSampleRate = -1;
			buffer = new Byte[16384 * 4];
			decompressor = null;

            this.initVolume = initVolume;

            //frames = 0;
            //diff = 0;
            //totalTime = 0;

            playTimer = new Timer(250);
            playTimer.Elapsed += PlayFrame;
            playTimer.Start();

			//StreamInfo();
		}

        private void StreamMp3(object state) {
            endOfStream = false;
            ReadFullyStream readFullyStream = null;

            Timer timeout = new Timer(Convert.ToDouble(Configs.Get("StreamTimeoutSec")) * 1000);
            timeout.AutoReset = false;
            timeout.Elapsed += (o, e) => {
                AddFrame(null, false);
            };

            String newTitle = null;
            bool changeNextFrame = false;
            int numFramesLoaded = 0;


            try {
                using (ShoutcastStream theStream = new ShoutcastStream(address)) {
                    readFullyStream = new ReadFullyStream(theStream);

                    theStream.StreamTitleChanged += (o, a) => {
                        newTitle = theStream.StreamTitle;
                        changeNextFrame = true;
                    };

                    do {

                        if (IsBufferFull()) {
                            Thread.Sleep(500);
                        } else {
                            Mp3Frame frame;
                            timeout.Start();

                            try {
                                frame = Mp3Frame.LoadFromStream(readFullyStream);
                                timeout.Stop();

                                AddFrame(frame, changeNextFrame, newTitle);

                                if (changeNextFrame) changeNextFrame = false;
                                numFramesLoaded++;
                            } catch (EndOfStreamException) {
                                timeout.Stop();

                                AddFrame(null, false);

                                endOfStream = true;
                                break;
                            }

                        }

                    } while (State != StreamState.Stopped && State != StreamState.Paused);
                }
            } catch (ThreadAbortException) {
                Util.Print(ConsoleColor.Yellow, "Warning", ": Stream thread aborted!");
            } finally {
                if (readFullyStream != null) {
                    readFullyStream.Close();
                    readFullyStream.Dispose();
                }
            }
        }

        private void AddFrame(Mp3Frame frame, bool changed = false, String streamTitle = null) {
			lock (addLock) {
                if (!isInitilized) {
                    waveOut = new WaveOut();

                    decompressor = CreateFrameDecompressor(frame);

                    bufferedWaveProvider = new BufferedWaveProvider(decompressor.OutputFormat);
                    bufferedWaveProvider.BufferDuration = TimeSpan.FromSeconds(MaxTimeInQueue); // allow us to get well ahead of ourselves
                    oldSampleRate = frame.SampleRate;

                    volumeProvider = new VolumeWaveProvider16(bufferedWaveProvider);
                    volumeProvider.Volume = initVolume;

                    waveOut.Init(volumeProvider);

                    isInitilized = true;
                }

				StreamFrame temp = new StreamFrame(frame);
				if (changed) {
					temp.Event = EventType.SongChange;
					temp.NewTitle = streamTitle;
				}
				if (tail == null) {
					playHead = head = tail = temp;
					BufferedTime += temp.FrameLengthSec;
					TotalTimeInQueue += temp.FrameLengthSec;
				} else {
					tail.Next = temp;
					temp.Prev = tail;
					tail = tail.Next;

					TotalTimeInQueue += temp.FrameLengthSec;
					BufferedTime += temp.FrameLengthSec;

					if (TotalTimeInQueue >= MaxTimeInQueue) {
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

		public List<EventTuple> GetAllEvents() {
			lock (addLock) {
				List<EventTuple> nums = new List<EventTuple>();

				StreamFrame cur = head;
				double time = 0;
				while (cur != null) {
					if (cur.Event != EventType.None)
						nums.Add(new EventTuple(cur.Event, State) {EventQueuePosition = time, CurrentTitle = cur.NewTitle});

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

		private void PurgeQueue() {
			while (head != null) {
				var temp = head.Next;
				if (temp != null)
					temp.Prev = null;

                head.Delete();
				head = temp;
			}
			tail = null;
			playHead = null;
            TotalTimeInQueue = 0;
        }

        private StreamFrame GetFrame() {
            StreamFrame ret = playHead;

            BufferedTime -= playHead.FrameLengthSec;
			playHead = playHead.Next;

			return ret;
		}

		public void Play() {
            if (State == StreamState.Stopped || State == StreamState.Paused) {
                State = StreamState.Playing;

                if (streamThread == null || !streamThread.IsAlive) {
                    streamThread = new Thread(StreamMp3);
                    streamThread.Start();
                }
            }
        }

        public void Pause() {
            if(State == StreamState.Playing || State == StreamState.Buffering) {
                State = StreamState.Paused;

                if (streamThread != null) {
                    streamThread.Abort();
                    streamThread = null;
                }
            }
        }

		public void Stop() {
            if (State == StreamState.Playing || State == StreamState.Paused || State == StreamState.Buffering) {
                State = StreamState.Stopped;

                if (streamThread != null) {
                    streamThread.Abort();
                    streamThread = null;
                }

                lock (playLock) {
                    PurgeQueue();
                    waveOut.Stop();
                }
            }
		}

		private void PlayFrame(Object o, EventArgs e) {
			lock (playLock) {
				//try {
					//Check if queue has shit in it.
					if (playHead == null) return;
					//Dont play if we're not playing.
					if (State == StreamState.Stopped || State == StreamState.Paused) return;

					//Don't play if we're buffering.
					if (BufferedTime < MinBufferdTime) {
                        if (State != StreamState.Buffering) {
                            if (OnStreamEvent != null)
                                OnStreamEvent(new EventTuple(EventType.StateChange, State));

                            State = StreamState.Buffering;
                        }
						return;
					}

					//We're now done buffering
					if (State == StreamState.Buffering) {
                        State = StreamState.Playing;

                        if (OnStreamEvent != null)
                            OnStreamEvent(new EventTuple(EventType.StateChange, State));
                    }

					//Get time in wave buffer and diffrence between that and what we want
					double inWavBuff = bufferedWaveProvider.BufferedDuration.TotalSeconds;
					double diff = NumSecToPlay - inWavBuff;

					if (diff <= 0)
						diff = 0.1;

					playTimer.Interval = diff * 1000;

					double totalTime = 0;
					double oldTotalTime = -1; //This prevents a deadlock
					//frames = 0;

					//Load frames into provider
					while (diff > 0 && oldTotalTime != totalTime) {
						oldTotalTime = totalTime;
						totalTime += playHead.FrameLengthSec;
						diff -= playHead.FrameLengthSec;

                        StreamFrame temp = GetFrame();
                        if (temp.Event != EventType.None && OnStreamEvent != null) {
                            if (temp.Event == EventType.StateChange) {
                                State = StreamState.Buffering;
                            } else if (temp.Event != EventType.StateChange) {
                                State = StreamState.Playing;
                            }
                            
                            //TODO: set EventQueuePosition?
                            OnStreamEvent(new EventTuple(temp.Event, State) { CurrentTitle = temp.NewTitle, EventQueuePosition = -1 });
                        }

                        AddFrameToWaveBuffer(temp.Frame);
						//frames++;
					}


                    if (waveOut.PlaybackState != PlaybackState.Playing)
						waveOut.Play();

					playTimer.Start();

				//} catch (NullReferenceException ex) { //happens when restarting the stream.
				///	Util.DumpException(ex);
				//}
			}
		}

        private void AddFrameToWaveBuffer(Mp3Frame frame) {
			if (frame != null) {
				if ((oldSampleRate != -1 && oldSampleRate != frame.SampleRate)) {
					// don't think these details matter too much - just help ACM select the right codec
					// however, the buffered provider doesn't know what sample rate it is working at
					// until we have a frame
					decompressor = CreateFrameDecompressor(frame);

					bufferedWaveProvider = new BufferedWaveProvider(decompressor.OutputFormat);
                    bufferedWaveProvider.BufferDuration = TimeSpan.FromSeconds(MaxTimeInQueue); // allow us to get well ahead of ourselves
					oldSampleRate = frame.SampleRate;
					//this.bufferedWaveProvider.BufferedDuration = 250;
				}

				//try {
					int decompressed = decompressor.DecompressFrame(frame, buffer, 0);
					bufferedWaveProvider.AddSamples(buffer, 0, decompressed);
				//}  catch (NAudio.MmException e) {
					//TODO: Handle stream exception
				//	Util.DumpException(e);
				//}
			}
		}

		private static IMp3FrameDecompressor CreateFrameDecompressor(Mp3Frame frame) {
			return new AcmMp3FrameDecompressor(new Mp3WaveFormat(frame.SampleRate, frame.ChannelMode == ChannelMode.Mono ? 1 : 2, frame.FrameLength, frame.BitRate));
		}

        /*
        private void StreamInfo() {
            var test = new System.Timers.Timer();
            test.Interval = 100;
            test.AutoReset = true;

            test.Elapsed += (o, e) => {
                lock (printLock) {
                    Console.Clear();
                    Util.PrintLine("Frames Played: ", ConsoleColor.Cyan, frames);
                    Util.PrintLine("Time Diffrence: ", ConsoleColor.White, String.Format("{0:0.000}", diff));
                    Util.PrintLine("Time Loaded: ", ConsoleColor.DarkYellow, String.Format("{0:0.000}", totalTime));
                    double BuffDelta = BufferedTime - oldBuffSecs;
                    double totes = bufferedWaveProvider != null ? bufferedWaveProvider.BufferedDuration.TotalSeconds : -1;
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
            //test.Start();
        }

        public void Draw() {
            var white = ConsoleColor.White;
            var green = ConsoleColor.Green;
            var black = ConsoleColor.Black;
            var yellow = ConsoleColor.Yellow;
            char b = '█';

            List<EventTuple> thing = GetAllEvents();

            double percentfull = (TotalTimeInQueue / MaxTimeInQueue) * 100;
            double percentBuff = (BufferedTime / TotalTimeInQueue);

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
                int pos = (int)Math.Max(Math.Round((e.EventQueuePosition / TotalTimeInQueue) * ((TotalTimeInQueue / MaxTimeInQueue) * 100)), 0);
                Console.CursorLeft = pos;
                if (e.Event == EventType.SongChange)
                    Util.Print(ConsoleColor.Cyan, "S");
                else if (e.Event == EventType.Disconnect)
                    Util.Print(ConsoleColor.Red, "D");
            }
        }
        */

        #region helper classes
        public class StreamFrame {
            public StreamFrame Next { get; set; }
            public StreamFrame Prev { get; set; }

            private Mp3Frame theFrame;
            public Mp3Frame Frame
            {
                get
                {
                    return theFrame;
                }
                set
                {
                    theFrame = value;
                    if (theFrame != null)
                        GetFrameLengthSec();
                }
            }
            public EventType Event { get; set; }
            public String NewTitle { get; set; }
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

                //Since it doesnt make any sence to have anything BUT a disconnect event 
                //represented in our queue, we can leave this just as a "StateChanged" event
                //without specifying the actual state.
                if (Frame == null)
                    Event = EventType.StateChange;
                else
                    Event = EventType.None;
            }

        }

        public class EventTuple {
            public EventTuple(EventType t, StreamState s) {
                Event = t;
                State = s;
            }

            public EventType Event { get; set; }
            public StreamState State { get; set; }
            public double EventQueuePosition { get; set; }
            public String CurrentTitle { get; set; }
        }
        #endregion
    }
}
