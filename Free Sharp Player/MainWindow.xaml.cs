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
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	/// 


	public partial class MainWindow : Window {
		[DllImport("kernel32")]
		static extern bool AllocConsole();

		Mp3Streamer theStreamer;
		System.Threading.Timer BufferTimer;

		public MainWindow() {
			InitializeComponent();

			AllocConsole();



			String address = "";

			using (WebClient wb = new WebClient()) {
				NameValueCollection data = new NameValueCollection();
				data["sid"] = "1"; //quality, 1 == 128, 2 == 360, 3 == 80

				//TODO: add ui to alert people this is loading
				wb.DownloadProgressChanged += (o, e) => {
					Console.WriteLine("S: " + e.BytesReceived);
				};

				Byte[] response = wb.UploadValues("http://radio.everfreeradio.com:5800/listen.pls", "POST", data);
				
				string[] responseData = System.Text.Encoding.UTF8.GetString(response, 0, response.Length).Split("\n".ToCharArray(), StringSplitOptions.None);


				//Todo: timeout, check for valid return data, find the adress in more dynamic way.
				address = responseData[2].Split("=".ToCharArray())[1];

				Console.WriteLine("Address: " + address);
			}

			theStreamer = new Mp3Streamer(address, 120);
			//theStreamer.Play();
			BufferLength.Maximum = 100;
			PlayLength.Maximum = 120;

			BufferTimer = new System.Threading.Timer((o) => {
				this.Dispatcher.Invoke(() => {
					BufferLength.Value = theStreamer.BufferFillPercent;
					PlayLength.Value = theStreamer.Pos ;
				});
			}, null, 0, 250);

		}

		/*//TODO: make this async?>
		private async void listen() {
			//TODO: handle timeouts.
			int bytes = 1;
			while (bytes != 0) { //kills the listener when we D/C
				Byte[] data = new Byte[1024];

				try {
					//if (connection.GetStream().DataAvailable)
					bytes = await connection.GetStream().ReadAsync(data, 0, data.Length);
				} catch (ObjectDisposedException) {
					break;
				}

				if (data != null) {
					string[] responseData = System.Text.Encoding.UTF8.GetString(data, 0, bytes).Split(new string[] { "\r\n" }, StringSplitOptions.None);


					foreach (String s in responseData) {
						Console.WriteLine(s);
					}

					//i++;
				}
			}
		}*/

		
		private void Play_Click(object sender, RoutedEventArgs e) {
			// bufferedWaveProvider
			//Play();
			theStreamer.Play();
		}

		private void Pause_Click(object sender, RoutedEventArgs e) {
			//Pause();
			theStreamer.Pause();
		}

		private void Stop_Click(object sender, RoutedEventArgs e) {
			//StopPlayback();
			theStreamer.Stop();
		}
	}
}
