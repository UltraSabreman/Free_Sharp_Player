using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Collections.Specialized;
using System.Threading;
using NAudio;
using NAudio.Wave;
using System.Diagnostics;
using System.Timers;


namespace Free_Sharp_Player {
	using Timer = System.Threading.Timer;

	public partial class MainWindow : Window {
		[DllImport("kernel32")]
		static extern bool AllocConsole();

		public enum StreamQuality {Low, Normal, High};

		MP3Stream theStreamer;
		Timer BufferTimer;

		public MainWindow() {
			InitializeComponent();

			AllocConsole();
			var payload = new Dictionary<string, object>() {
				{ "action", "queue" },
			};

			var thing = HttpPostRequest.SecureAPICall(payload);


			BufferLength.Maximum = 100;
			//PlayLength.Maximum = 120;

			ConnectToStream(StreamQuality.Normal);

			BufferTimer = new Timer((o) => {
				this.Dispatcher.Invoke(() => {
					if (BufferLength == null) {
						Thread.CurrentThread.Abort();
						return;
					}
					BufferLength.Value = theStreamer.BufferFillPercent;
					//PlayLength.Value = theStreamer.Pos ;
				});
			}, null, 0, 250);

			Play_Click(null, null);
		}

		private void ConnectToStream(StreamQuality Quality) {
			String address = "";
			bool Connected = false;

			while (!Connected) {
				using (WebClient wb = new WebClient()) {
					NameValueCollection data = new NameValueCollection();
					data["sid"] = (Quality == StreamQuality.Normal ? "1" : (Quality == StreamQuality.Low ? "3" : "2"));

					//TODO: add ui to alert people this is loading
					Byte[] response = wb.UploadValues("http://radio.everfreeradio.com:5800/listen.pls", "POST", data);

					string[] responseData = System.Text.Encoding.UTF8.GetString(response, 0, response.Length).Split("\n".ToCharArray(), StringSplitOptions.None);

					//Todo: timeout, check for valid return data, find the adress in more dynamic way.
					address = responseData[2].Split("=".ToCharArray())[1];
				}

				try {
					theStreamer = new MP3Stream(address, 30);
					theStreamer.OnStreamTitleChange += (t, a) => {
						this.Dispatcher.Invoke(new Action(() => {
							StreamTitleLabel.Content = t;
							StreamArtistLabel.Content = a;
						}));
					};
					Connected = true;
				} catch (Exception) { }
			}

		}
		
		private void Play_Click(object sender, RoutedEventArgs e) {
			if (theStreamer.PlaybackState == MP3Stream.StreamingPlaybackState.Playing) {
				theStreamer.Pause();
				PlayButton.Content = "►";
			} else {
				theStreamer.Play();
				PlayButton.Content = "▌▐";
			}
		}

		private void Stop_Click(object sender, RoutedEventArgs e) {
			theStreamer.Stop();
			PlayButton.Content = "►";
		}

		private void Window_KeyUp(object sender, KeyEventArgs e) {
			if (e.Key == Key.Escape)
				Application.Current.Shutdown();
		}

		private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
			if (theStreamer == null) return;
			theStreamer.Volume = (float)(sender as Slider).Value / 100;
		}

		private void Window_Closed(object sender, EventArgs e) {
			BufferTimer.Dispose();
			theStreamer.Stop();
			Application.Current.Shutdown();
		}


		
	}
}
