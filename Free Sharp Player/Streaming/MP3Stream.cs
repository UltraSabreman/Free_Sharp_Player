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

		public delegate void TitleChange(String title, String artist);
		public event TitleChange OnStreamTitleChange;

		public delegate void BufferChange(int pos);
		public event BufferChange OnBufferChange;

		public PlaybackState StreamState { get; private set; }
		public bool EndOfStream { get; private set; }
		//public float BufferFillPercent { get; private set; }
		public float Volume { get; set; }

		public double BufferLen { get { return theQ.MaxBufferLengthSec; } }
		public String StreamTitle { get; private set; }

		private String address;
		private Thread streamThread;
		private Timer durationUpdate;

		private StreamQueue theQ;
		private bool changeNextFrame;
		private String newTitle;

		public MP3Stream(String address, int BufferSize = 20) {
			Util.ToggleAllowUnsafeHeaderParsing(true);

			//this simply checks that the given address actualy works
			//An exception is thrown if it doesn't
			((HttpWebRequest)HttpWebRequest.Create(address)).GetResponse().Close();

			this.address = address;

			Volume = 0.5f;

			theQ = new StreamQueue();
			theQ.OnStreamTitleChange += () => {
				StreamTitle = newTitle;
				if (OnStreamTitleChange != null)
					OnStreamTitleChange(newTitle, "");

			
			};

			durationUpdate = new Timer(1000);
			durationUpdate.AutoReset = true;
			durationUpdate.Enabled = true;
			durationUpdate.Elapsed += (o, e) => {
				if (theQ == null || theQ.MaxBufferLengthSec <= 0 || OnBufferChange == null) return;
				int percent = (int)Math.Round((theQ.CurBufferLengthSec / theQ.MaxBufferLengthSec) * 100);
				OnBufferChange(percent);
			};
			durationUpdate.Start();


			changeNextFrame = true;
			newTitle = "";
		}

		public void Play() {
			StreamState = PlaybackState.Playing;

			if (streamThread == null || !streamThread.IsAlive) {
				streamThread = new Thread(StreamMp3);
				streamThread.Start();
			}
			theQ.Play();
		}

		public void Pause() {
			theQ.Pause();
			StreamState = theQ.StreamState;
		}

		public void Stop() {
			if (streamThread != null) {
				streamThread.Abort();
				streamThread = null;
			}
			theQ.Stop();
			StreamState = theQ.StreamState;
		}

		private void StreamMp3(object state) {
			EndOfStream = false;
			ReadFullyStream readFullyStream = null;

			try {
				using (ShoutcastStream theStream = new ShoutcastStream(address)) {
					theStream.StreamTitleChanged += (o,a) => {
						newTitle = theStream.StreamTitle;
						changeNextFrame = true;
					};

					readFullyStream = new ReadFullyStream(theStream);

					do {

						/*try {
							double t = (double)bufferedWaveProvider.BufferedBytes / (double)bufferedWaveProvider.BufferLength;
							int posnow = (int)(t * 100);
							if (oldBufferLen != posnow && OnBufferChange != null)
								OnBufferChange(posnow);
						} catch (Exception) {
							if (OnBufferChange != null)
								OnBufferChange(0);
						}*/

						if (theQ.IsBufferFull()) {
							Thread.Sleep(500);
						} else {
							Mp3Frame frame;
							try {
								frame = Mp3Frame.LoadFromStream(readFullyStream);
								theQ.AddFrame(frame, changeNextFrame);

								if (changeNextFrame) changeNextFrame = false;
							} catch (EndOfStreamException) {
								EndOfStream = true;
								// reached the end of the MP3 file / stream
								break;
							}

						}

					} while (StreamState != PlaybackState.Stopped);
					Util.PrintLine(ConsoleColor.Red, "DoneAdding");

				}
			} catch (ThreadAbortException) {
				Util.Print(ConsoleColor.DarkYellow, "Warning", ": Stream thread aborted!");
			} finally {
				if (readFullyStream != null) {
					readFullyStream.Close();
					readFullyStream.Dispose();
				}
			}
		}


	}
}
