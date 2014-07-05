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
		private enum StreamingPlaybackState { Stopped, Playing, Buffering, Paused }


		private StreamingPlaybackState PlaybackState { get; set; }
		public bool EndOfStream { get; private set; }
		public float BufferFillPercent { get; private set; }

		public float Pos { get; private set; }

		private int bufferSizeSec = 0;
		private HttpWebRequest mainRequest;
		private IWavePlayer waveOut;
		private Timer waveLoader;
		private VolumeWaveProvider16 volumeProvider;
		private BufferedWaveProvider bufferedWaveProvider;
		private String Address;

		public Mp3Streamer(String address, int BufferSize = 20) {
			waveLoader = new Timer(250);
			waveLoader.Elapsed += waveLoader_Tick;
			waveLoader.AutoReset = true;

			Address = address;
			bufferSizeSec = BufferSize;
		}

		public void Play() {
			//if (PlaybackState == StreamingPlaybackState.Buffering) return;

			PlaybackState = StreamingPlaybackState.Playing;

			ThreadPool.QueueUserWorkItem(StreamMp3, Address);

			waveLoader.Enabled = true;

			while (waveOut == null)
				Thread.Sleep(250);

			waveOut.Play();
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
					mainRequest.Abort();
				

				PlaybackState = StreamingPlaybackState.Stopped;
				if (waveOut != null) {
					waveOut.Stop();
					waveOut.Dispose();
					waveOut = null;
				}

				waveLoader.Enabled = false;
				Thread.Sleep(500);
				BufferFillPercent = 0;
			}
		}




		private void StreamMp3(object state) {
			EndOfStream = false;
			mainRequest = (HttpWebRequest)WebRequest.Create((String)state);
			HttpWebResponse resp;

			try {
				//webRequest.KeepAlive = false;
				resp = (HttpWebResponse)mainRequest.GetResponse();
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
						Pos = readFullyStream.Position;
						if (IsBufferNearlyFull()) {
							Debug.WriteLine("Buffer getting full, taking a break");
							Thread.Sleep(500);
						} else {
							Mp3Frame frame;
							try {
								frame = Mp3Frame.LoadFromStream(readFullyStream);
							} catch (EndOfStreamException) {
								EndOfStream = true;
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
								bufferedWaveProvider.BufferDuration = TimeSpan.FromSeconds(bufferSizeSec); // allow us to get well ahead of ourselves
								//this.bufferedWaveProvider.BufferedDuration = 250;
							}
							int decompressed = decompressor.DecompressFrame(frame, buffer, 0);
							//Debug.WriteLine(String.Format("Decompressed a frame {0}", decompressed));
							bufferedWaveProvider.AddSamples(buffer, 0, decompressed);
						}

					} while (PlaybackState != StreamingPlaybackState.Stopped);
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
			return new AcmMp3FrameDecompressor(new Mp3WaveFormat(frame.SampleRate, frame.ChannelMode == ChannelMode.Mono ? 1 : 2, frame.FrameLength, frame.BitRate));
		}


		private bool IsBufferNearlyFull() {
			return bufferedWaveProvider != null 
				&& bufferedWaveProvider.BufferLength - bufferedWaveProvider.BufferedBytes 
				< bufferedWaveProvider.WaveFormat.AverageBytesPerSecond / 4;
		}

		private void waveLoader_Tick(object sender, EventArgs e) {
			if (PlaybackState != StreamingPlaybackState.Stopped) {
				if (waveOut == null && bufferedWaveProvider != null) {
					Debug.WriteLine("Creating WaveOut Device");
					waveOut = new WaveOut();
					//waveOut.PlaybackStopped += OnPlaybackStopped;
					volumeProvider = new VolumeWaveProvider16(bufferedWaveProvider);
					//TODO: set volume
					//volumeProvider.Volume = volumeSlider1.Volume;
					waveOut.Init(volumeProvider);
					//TODO: Buffer Length

				} else if (bufferedWaveProvider != null) {
					var bufferedSeconds = bufferedWaveProvider.BufferedDuration.TotalSeconds;
					BufferFillPercent = (int)((bufferedSeconds/ (double)bufferSizeSec) * 100);

					// make it stutter less if we buffer up a decent amount before playing
					if (bufferedSeconds < 0.5 && PlaybackState == StreamingPlaybackState.Playing && !EndOfStream) {
						Buffer();
					} else if (bufferedSeconds > 4 && PlaybackState == StreamingPlaybackState.Buffering) {
						Play();
					} else if (EndOfStream && bufferedSeconds == 0) {
						Debug.WriteLine("Reached end of stream");
						Stop();
					}
				}

			}
		}


		private void ShowError(String msg) {
			MessageBox.Show(msg, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
		}

	}
}
