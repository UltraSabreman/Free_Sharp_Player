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
	class Mp3Streamer {
		enum StreamingPlaybackState { Stopped, Playing, Buffering, Paused }


		private volatile StreamingPlaybackState playbackState { get; set; }
		private bool endOfStream { get; private set; }

		private HttpWebRequest webRequest;
		private IWavePlayer waveOut;
		private Timer waveLoader;
		private VolumeWaveProvider16 volumeProvider;
		private BufferedWaveProvider bufferedWaveProvider;
		private String Address;

		public Mp3Streamer(String address) {
			waveLoader = new Timer(250);
			waveLoader.Elapsed += waveLoader_Tick;
			waveLoader.AutoReset = true;

			Address = address;

		}

		public void Play() {
			ThreadPool.QueueUserWorkItem(StreamMp3, Address);

			waveOut.Play();
			Debug.WriteLine(String.Format("Started playing, waveOut.PlaybackState={0}", waveOut.PlaybackState));
			playbackState = StreamingPlaybackState.Playing;

		}

				private void StopPlayback() {
			if (playbackState != StreamingPlaybackState.Stopped) {
				if (!fullyDownloaded) {
					webRequest.Abort();
				}

				playbackState = StreamingPlaybackState.Stopped;
				if (waveOut != null) {
					waveOut.Stop();
					waveOut.Dispose();
					waveOut = null;
				}
				timer1.Enabled = false;
				// n.b. streaming thread may not yet have exited
				Thread.Sleep(500);
				ShowBufferState(0);
			}
		}
		private void Pause() {
			playbackState = StreamingPlaybackState.Buffering;
			waveOut.Pause();
			Debug.WriteLine(String.Format("Paused to buffer, waveOut.PlaybackState={0}", waveOut.PlaybackState));
		}


		private void StreamMp3(object state) {
			fullyDownloaded = false;
			String url = (String)state;
			webRequest = (HttpWebRequest)WebRequest.Create(url);
			HttpWebResponse resp;
			try {
				//webRequest.KeepAlive = false;
				resp = (HttpWebResponse)webRequest.GetResponse();
			} catch (WebException e) {
				if (e.Status != WebExceptionStatus.RequestCanceled) {
					ShowError(e.Message);
				}
				return;
			}
			Byte[] buffer = new Byte[16384 * 4]; // needs to be big enough to hold a decompressed frame

			IMp3FrameDecompressor decompressor = null;
			try {
				using (Stream responseStream = resp.GetResponseStream()) {
					ReadFullyStream readFullyStream = new ReadFullyStream(responseStream);
					do {
						if (IsBufferNearlyFull) {
							Debug.WriteLine("Buffer getting full, taking a break");
							Thread.Sleep(500);
						} else {
							Mp3Frame frame;
							try {
								frame = Mp3Frame.LoadFromStream(readFullyStream);
							} catch (EndOfStreamException) {
								fullyDownloaded = true;
								// reached the end of the MP3 file / stream
								break;
							} catch (WebException) {
								// probably we have aborted download from the GUI thread
								break;
							}
							if (decompressor == null) {
								// don't think these details matter too much - just help ACM select the right codec
								// however, the buffered provider doesn't know what sample rate it is working at
								// until we have a frame
								decompressor = CreateFrameDecompressor(frame);
								bufferedWaveProvider = new BufferedWaveProvider(decompressor.OutputFormat);
								bufferedWaveProvider.BufferDuration = TimeSpan.FromSeconds(20); // allow us to get well ahead of ourselves
								//this.bufferedWaveProvider.BufferedDuration = 250;
							}
							int decompressed = decompressor.DecompressFrame(frame, buffer, 0);
							//Debug.WriteLine(String.Format("Decompressed a frame {0}", decompressed));
							bufferedWaveProvider.AddSamples(buffer, 0, decompressed);
						}

					} while (playbackState != StreamingPlaybackState.Stopped);
					Debug.WriteLine("Exiting");
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
			WaveFormat waveFormat = new Mp3WaveFormat(frame.SampleRate, frame.ChannelMode == ChannelMode.Mono ? 1 : 2,
				frame.FrameLength, frame.BitRate);
			return new AcmMp3FrameDecompressor(waveFormat);
		}


		private bool IsBufferNearlyFull {
			get {
				return bufferedWaveProvider != null &&
					   bufferedWaveProvider.BufferLength - bufferedWaveProvider.BufferedBytes
					   < bufferedWaveProvider.WaveFormat.AverageBytesPerSecond / 4;
			}
		}

		private void waveLoader_Tick(object sender, EventArgs e) {
			if (playbackState != StreamingPlaybackState.Stopped) {
				if (waveOut == null && bufferedWaveProvider != null) {
					Debug.WriteLine("Creating WaveOut Device");
					waveOut = new WaveOut();
					//waveOut.PlaybackStopped += OnPlaybackStopped;
					volumeProvider = new VolumeWaveProvider16(bufferedWaveProvider);
					//volumeProvider.Volume = volumeSlider1.Volume;
					waveOut.Init(volumeProvider);
					//TODO: Buffer Length
					/*this.Dispatcher.Invoke(() => {
						BufferLength.Maximum = (int)bufferedWaveProvider.BufferDuration.TotalMilliseconds;
					});*/
				} else if (bufferedWaveProvider != null) {
					var bufferedSeconds = bufferedWaveProvider.BufferedDuration.TotalSeconds;
					ShowBufferState(bufferedSeconds);
					// make it stutter less if we buffer up a decent amount before playing
					if (bufferedSeconds < 0.5 && playbackState == StreamingPlaybackState.Playing && !fullyDownloaded) {
						Pause();
					} else if (bufferedSeconds > 4 && playbackState == StreamingPlaybackState.Buffering) {
						Play();
					} else if (fullyDownloaded && bufferedSeconds == 0) {
						Debug.WriteLine("Reached end of stream");
						StopPlayback();
					}
				}

			}
		}
		private void ShowBufferState(double totalSeconds) {
			//labelBuffered.Text = String.Format("{0:0.0}s", totalSeconds);
			/*this.Dispatcher.Invoke(() => {
				BufferLength.Value = (int)(totalSeconds * 1000);
			});*/
		}


		private void ShowError(String msg) {
			MessageBox.Show(msg, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
		}

	}
}
