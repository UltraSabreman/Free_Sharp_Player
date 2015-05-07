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
		public String StreamTitle { get; private set; }

		private String address;
		private Thread streamThread;
		private Timer durationUpdate;

		private StreamQueue theQ;
		private bool changeNextFrame;
		private String newTitle;

		public MP3Stream(String address, StreamQueue q) {
			Util.ToggleAllowUnsafeHeaderParsing(true);

			//this simply checks that the given address actualy works
			//An exception is thrown if it doesn't
			//TODO: replace with ping.
			//((HttpWebRequest)HttpWebRequest.Create(address)).GetResponse().Close();

			this.address = address;

			theQ = q;

			changeNextFrame = true;
			newTitle = "";
		}

		public void Play() {
			IsPlaying = true;

			if (streamThread == null || !streamThread.IsAlive) {
				streamThread = new Thread(StreamMp3);
				streamThread.Start();
			}

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
					theStream.StreamTitleChanged += (o,a) => {
						newTitle = theStream.StreamTitle;
						changeNextFrame = true;
					};

					readFullyStream = new ReadFullyStream(theStream);
					int numFramesLoaded = 0;
					do {

						if (theQ.IsBufferFull()) {
							Thread.Sleep(500);
						} else {
							Mp3Frame frame;
							var timeout = new Timer();
							timeout.Interval = 5000; //TODO: make this configurable
							timeout.AutoReset = false;
							timeout.Enabled = true;
							timeout.Elapsed += (o, e) => {
								theQ.AddFrame(null, false);
							};

							try {

								frame = Mp3Frame.LoadFromStream(readFullyStream);
								timeout.Enabled = false;
								timeout.Stop();

								theQ.AddFrame(frame, changeNextFrame, newTitle);

								if (changeNextFrame) changeNextFrame = false;
								numFramesLoaded++;
							} catch (EndOfStreamException) {
								timeout.Enabled = false;
								timeout.Stop();

								theQ.AddFrame(null, false);

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

			/*catch (Exception e) {
				Util.DumpException(e);
				throw e;
			}*/
		}


	}
}
