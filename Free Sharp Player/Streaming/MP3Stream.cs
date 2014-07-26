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
	class MP3Stream {
		public enum StreamingPlaybackState { Stopped, Playing, Buffering, Paused }

		public delegate void TitleChange(String title, String artist);
		public event TitleChange OnStreamTitleChange;

		public StreamingPlaybackState PlaybackState { get; private set; }
		public bool EndOfStream { get; private set; }
		public float BufferFillPercent { get; private set; }
		public float Volume { get; set; }
		public float Pos { get; private set; }
		public String StreamTitle { get; private set; }

		private int bufferSizeSec = 0;
		private IWavePlayer waveOut;
		private Timer waveLoader;
		private VolumeWaveProvider16 volumeProvider;
		private BufferedWaveProvider bufferedWaveProvider;
		private String address;
		private Thread streamThread;

		public MP3Stream(String address, int BufferSize = 20) {
			Util.ToggleAllowUnsafeHeaderParsing(true);

			//this simply checks that the given address actualy works
			//An exception is thrown if it doesn't
			((HttpWebRequest)HttpWebRequest.Create(address)).GetResponse().Close();

			waveLoader = new Timer(250);
			waveLoader.Elapsed += waveLoader_Tick;
			waveLoader.AutoReset = true;

			this.address = address;
			bufferSizeSec = BufferSize;

			Volume = 0.0f;
		}

		public void Play() {
			PlaybackState = StreamingPlaybackState.Playing;

			if (streamThread == null || !streamThread.IsAlive) {
				streamThread = new Thread(StreamMp3);
				streamThread.Start();
			}

			waveLoader.Enabled = true;

			while (waveOut == null)
				Thread.Sleep(250);

			try {
				waveOut.Play();
			} catch (Exception) { }
		}

		public void Pause() {
			PlaybackState = StreamingPlaybackState.Paused;

			waveOut.Pause();
		}

		private void Buffer() {
			PlaybackState = StreamingPlaybackState.Buffering;

			waveOut.Pause();
		}

		public void Stop() {
			if (PlaybackState != StreamingPlaybackState.Stopped) {
				if (!EndOfStream)
					streamThread.Abort();

				PlaybackState = StreamingPlaybackState.Stopped;
				if (waveOut != null) {
					waveOut.Stop();
					waveOut.Dispose();
					waveOut = null;
				}

				if (bufferedWaveProvider != null)
					bufferedWaveProvider.ClearBuffer();


				waveLoader.Enabled = false;
				Thread.Sleep(500);
				BufferFillPercent = 0;
			}
		}




		private void StreamMp3(object state) {
			EndOfStream = false;

			int oldSampleRate = -1;
			Byte[] buffer = new Byte[16384 * 4]; // needs to be big enough to hold a decompressed frame
			IMp3FrameDecompressor decompressor = null;

			try {
				using (ShoutcastStream theStream = new ShoutcastStream(address)) {
					
					ReadFullyStream readFullyStream = new ReadFullyStream(theStream);
					//ShoutcastStream readFullyStream = new ShoutcastStream(theStream, metaInt);

					int i = 0;
					do {
						i++;
						//Pos = theStream.Position;
						if (IsBufferNearlyFull()) 
							Thread.Sleep(500);
						else {
							Mp3Frame frame;
							try {
								frame = Mp3Frame.LoadFromStream(readFullyStream);
							} catch (EndOfStreamException) {
								EndOfStream = true;
								// reached the end of the MP3 file / stream
								break ;
							}
							if (frame != null) {
								if (decompressor == null || (oldSampleRate != -1 && oldSampleRate != frame.SampleRate)) {
									// don't think these details matter too much - just help ACM select the right codec
									// however, the buffered provider doesn't know what sample rate it is working at
									// until we have a frame
									decompressor = CreateFrameDecompressor(frame);

									bufferedWaveProvider = new BufferedWaveProvider(decompressor.OutputFormat);
									bufferedWaveProvider.BufferDuration = TimeSpan.FromSeconds(bufferSizeSec); // allow us to get well ahead of ourselves
									oldSampleRate = frame.SampleRate;
									//this.bufferedWaveProvider.BufferedDuration = 250;
								}


								int decompressed = decompressor.DecompressFrame(frame, buffer, 0);
								bufferedWaveProvider.AddSamples(buffer, 0, decompressed);
							}
						}

					} while (PlaybackState != StreamingPlaybackState.Stopped);
					// was doing this in a finally block, but for some reason
					// we are hanging on response stream .Dispose so never get there
					decompressor.Dispose();
				}
			} finally {
				if (decompressor != null) {
					decompressor.Dispose();
				}
			}
		}

		private static IMp3FrameDecompressor CreateFrameDecompressor(Mp3Frame frame) {
			return new AcmMp3FrameDecompressor(new Mp3WaveFormat(frame.SampleRate, frame.ChannelMode == ChannelMode.Mono ? 1 : 2, frame.FrameLength, frame.BitRate));
		}


		private bool IsBufferNearlyFull() {
			bool test = bufferedWaveProvider != null
				&& ((bufferedWaveProvider.BufferLength - bufferedWaveProvider.BufferedBytes)
				< bufferedWaveProvider.WaveFormat.AverageBytesPerSecond / 4);
			return test;
		}

		private void waveLoader_Tick(object sender, EventArgs e) {
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
		}

		private void DumpStreamStats(double bufferedSeconds) {
			Console.SetCursorPosition(0, 0);

			Console.Write("Buffer Status: ");
			Console.ForegroundColor = ConsoleColor.Green;
			Console.Write(waveOut.PlaybackState.ToString());
			Console.ForegroundColor = ConsoleColor.Gray;
			Console.Write(" | ");
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine(Math.Round(bufferedSeconds) + "s");
			Console.ForegroundColor = ConsoleColor.Gray;

			Console.Write("Title: ");
			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.WriteLine(StreamTitle);
			Console.ForegroundColor = ConsoleColor.Gray;

			Console.Write("Volume: ");
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.WriteLine(Math.Round(Volume * 100));
			Console.ForegroundColor = ConsoleColor.Gray;
		}


		private void ShowError(String msg) {
			MessageBox.Show(msg, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
		}

	}
}
