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

//Todo: fix song timing
//Todo: fix retarted locups

namespace Free_Sharp_Player {
	using Timer = System.Timers.Timer;
	using Newtonsoft.Json;
	using System.Windows.Media.Animation;

	public partial class MainWindow : Window {
		private Object trackLock = new Object();
		private Object radioLock = new Object();

		[DllImport("kernel32")]
		static extern bool AllocConsole();

		public bool IsPlaying { get; private set; }

		private Timer doubleClickCheck = new Timer(350);
		private bool isDoubleClicking = false;

		//Timer Updater = new Timer(1000);

		private StreamManager streamManager;

		private MainModel mainModel;
		private VolumeModel volumeModel;
		private ExtraMenuModel extraModel;
		private PlaylistModel playlistModel;

		public MainWindow() {
			AllocConsole();

			InitializeComponent();
			btn_PlayPause.IsEnabled = false;

			doubleClickCheck.Elapsed += (o, e) => {
				isDoubleClicking = false;
			};

			new Thread(() => {
				ConnectToStream(StreamQuality.Normal);
				//streamManager.UpdateData(null);

				Dispatcher.Invoke(new Action(() => {
					mainModel = new MainModel(this, streamManager);
					volumeModel = new VolumeModel(this, streamManager);
					extraModel = new ExtraMenuModel(this, streamManager);
					playlistModel = new PlaylistModel(this, streamManager);

					btn_PlayPause.IsEnabled = true;
					Connecting.Visibility = System.Windows.Visibility.Collapsed;
				}));

			}).Start();
		}


		private void ConnectToStream(StreamQuality Quality) {
			String address = "";
			bool Connected = false;

			while (!Connected) {

				getRadioInfo temp = getRadioInfo.doPost();

				using (WebClient wb = new WebClient()) {
					NameValueCollection data = new NameValueCollection();
					String tempAddr = temp.servers.medQuality.Split("?".ToCharArray())[0];
					data["sid"] = temp.servers.medQuality.Split("=".ToCharArray())[1];//(Quality == StreamQuality.Normal ? "1" : (Quality == StreamQuality.Low ? "3" : "2"));

					Byte[] response = wb.UploadValues(tempAddr, "POST", data);

					string[] responseData = System.Text.Encoding.UTF8.GetString(response, 0, response.Length).Split("\n".ToCharArray(), StringSplitOptions.None);

					//Todo: timeout, check for valid return data, find the adress in more dynamic way.
					address = responseData[2].Split("=".ToCharArray())[1];
				}


				streamManager = new StreamManager(address);

				Connected = true;
			}

		}


		//TODO: run on event

		public void MainTick(Object o, EventArgs e) {
			new Thread(() => { }).Start();

			//TODO: main song tick

			//mainModel.UpdateSongProgress(playlistModel.Playing, playlistModel.Played[0], theStreamer.startTime, radioInfo);
		}


		public void Play() { IsPlaying = true; streamManager.Play(); }
		public void Stop() { 
			IsPlaying = false; 
			streamManager.Stop();
			Dispatcher.Invoke(new Action(() => {
				MyBuffer.Update(null);
			}));
		}

		public void SetVolume(double Volume) {
			if (Volume < 0 || Volume > 100) throw new ArgumentOutOfRangeException("Volume", Volume, "Volume must be between 0 and 100");

			if (streamManager != null)
				streamManager.Volume = (float)Volume / 100;
		}



		private void Grid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
			//currentSong.trackID;
		}
		private void Window_KeyUp(object sender, KeyEventArgs e) {
			if (e.Key == Key.Escape)
				Application.Current.Shutdown();
		}

		private void Window_Closed(object sender, EventArgs e) {
			streamManager.Stop();
			Application.Current.Shutdown();
		}

		private void Grid_MouseDown(object sender, MouseButtonEventArgs e) {
			if (e.ChangedButton == MouseButton.Left) {
				this.DragMove();
				if (!isDoubleClicking) {
					isDoubleClicking = true;
					doubleClickCheck.Start();
				} else {
					playlistModel.AnimateLists();
					isDoubleClicking = false;
					doubleClickCheck.Stop();
				}
			}
		}

	}
}
