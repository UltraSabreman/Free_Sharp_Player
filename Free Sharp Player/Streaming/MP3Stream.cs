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
	public class MP3Stream {


		public bool IsPlaying { get; private set; }
		public bool EndOfStream { get; private set; }

		private Track CurrentTrack;
		private getRadioInfo CurrentInfo;
		private EventTuple normalEvent;

		private String address;
		private Thread streamThread;
		private Timer timeOut;

		private StreamQueue theQ;
		private bool changeNextFrame;
		private bool firstRun = true;

		public MP3Stream(String address, StreamQueue q) {
			Util.ToggleAllowUnsafeHeaderParsing(true);

			//this simply checks that the given address actualy works
			//An exception is thrown if it doesn't
			//TODO: replace with ping.
			//((HttpWebRequest)HttpWebRequest.Create(address)).GetResponse().Close();

			this.address = address;

			timeOut = new Timer();
			timeOut.Interval = 5000; //TODO: make this configurable
			timeOut.AutoReset = false;
			timeOut.Enabled = true;
			timeOut.Elapsed += (o, e) => {
				theQ.AddFrame(null, new EventTuple() { Event = EventType.Disconnect });
			};

			theQ = q;

			changeNextFrame = true;
			CurrentTrack = null;
		}

		public void Play() {
			IsPlaying = true;

			if (streamThread == null || !streamThread.IsAlive) {
				streamThread = new Thread(StreamMp3);
				streamThread.Start();
			}

			//SongChanged(null, null);
		}


		public void Stop() {
			IsPlaying = false;

			if (streamThread != null) {
				streamThread.Abort();
				streamThread = null;
			}
		}

		private void StreamMp3(object state) {
			EndOfStream = false;
			ReadFullyStream readFullyStream = null;

			try {
				using (ShoutcastStream theStream = new ShoutcastStream(address)) {
					theStream.StreamTitleChanged += SongChanged;

					readFullyStream = new ReadFullyStream(theStream);
					int numFramesLoaded = 0;
					do {

						if (theQ.IsBufferFull()) {
							Thread.Sleep(500);
						} else {
							Mp3Frame frame;

							timeOut.Enabled = true;
							timeOut.Start();

							try {

								frame = Mp3Frame.LoadFromStream(readFullyStream);
								timeOut.Enabled = false;
								timeOut.Stop();

								if (changeNextFrame) {
									theQ.AddFrame(frame, new EventTuple() { CurrentSong = CurrentTrack, RadioInfo = CurrentInfo, Event = EventType.SongChange });
									changeNextFrame = false;
									normalEvent = null;
								} else {
									if (normalEvent == null)
										normalEvent = new EventTuple() { CurrentSong = CurrentTrack, RadioInfo = CurrentInfo, Event = EventType.None };
									theQ.AddFrame(frame, normalEvent);
								}

								if (firstRun) {
									SongChanged(null, null);
									firstRun = false;
								}
								numFramesLoaded++;
							} catch (EndOfStreamException) {
								timeOut.Enabled = false;
								timeOut.Stop();

								theQ.AddFrame(null, new EventTuple() { Event = EventType.Disconnect });

								EndOfStream = true;
								break;
							}

						}

					} while (IsPlaying);
					//TODO: Handle stream errors (Aborts + crashes)
				}
			} catch (ThreadAbortException) {
				Util.Print(ConsoleColor.Yellow, "Warning", ": Stream thread aborted!");
			}  finally {
				if (readFullyStream != null) {
					readFullyStream.Close();
					readFullyStream.Dispose();
				}
			}
		}

		private void SongChanged(Object o, EventArgs e) {
			Util.PrintLine(ConsoleColor.Green, "Song Changed");
			CurrentInfo = getRadioInfo.doPost();
			if (CurrentInfo == null || String.IsNullOrEmpty(CurrentInfo.track_id) || CurrentInfo.track_id == "0")
				CurrentTrack = new Track() { trackID = "0", title = CurrentInfo.title };
			else {  //This wont be null if we get this far, because it means the track does exist in their database
				var trackList =  getTrack.doPost(int.Parse(CurrentInfo.track_id)).track;
				if (trackList == null || trackList.Count == 0)
					CurrentTrack = new Track() { trackID = "0", title = CurrentInfo.title };
				else 
					CurrentTrack = trackList.First();
			}

			changeNextFrame = true;
		}

	}
}
