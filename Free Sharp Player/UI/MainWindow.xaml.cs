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

//TODO: hanlde network disconnects
//TODO: sanity check duration value on now playing.
//tODO: make icons for times show up on main bar when epxaneded? (or not
//make extra menu apear on right click of anything.
//replace extra icon with app icon?
//how to handle requester?
//Elipse song names in playlist/queue (wiht alpha gradient?)
//add calculated song progress bar somehwre in playing (and time to?)
//make votes easier to see?
//prevent player from going of screen when dragged (rest like that pandora client)
//when playlist expands, move the player away from the screen edge the needed amount (possibly back when collapses).


namespace Free_Sharp_Player {
	using Timer = System.Timers.Timer;
	using Newtonsoft.Json;
	using System.Windows.Media.Animation;

	public partial class MainWindow : Window {
		[DllImport("kernel32")]
		static extern bool AllocConsole();

		public enum StreamQuality {Low, Normal, High};
		Timer doubleClickCheck = new Timer(750);
		bool isDoubleClicking = false;
		public MP3Stream theStreamer;

		Timer Updater = new Timer(1000);

		private MainModel mainModel;
		private VolumeModel volumeModel;
		private ExtraMenuModel extraModel;
		private PlaylistModel playlistModel;

		public MainWindow() {
			InitializeComponent();

			doubleClickCheck.Elapsed += (o, e) => {
				isDoubleClicking = false;
			};


			AllocConsole();

			mainModel = new MainModel(this);
			volumeModel = new VolumeModel(this);
			extraModel = new ExtraMenuModel(this);
			playlistModel = new PlaylistModel(this);

			Updater.Elapsed += mainModel.Tick;
			Updater.Elapsed += volumeModel.Tick;
			Updater.Elapsed += extraModel.Tick;
			Updater.Elapsed += playlistModel.Tick;
			Updater.Elapsed += MainTick;
			Updater.AutoReset = true;

			ConnectToStream(StreamQuality.Normal);
			Updater.Start();
		}

		public void MainTick(Object o, EventArgs e) {
			mainModel.UpdateSongProgress(playlistModel.Playing, playlistModel.Played[0], theStreamer.BufferLen);
		}


		public MP3Stream.StreamingPlaybackState GetStreamState() {
			return theStreamer.PlaybackState;
		}

		public void Play() { theStreamer.Play(); }
		public void Pause() { theStreamer.Pause(); }
		public void Stop() { theStreamer.Stop(); }

		public void SetVolume(double Volume) {
			if (Volume < 0 || Volume > 100) throw new ArgumentOutOfRangeException("Volume", Volume, "Volume must be between 0 and 100");

			if (theStreamer == null) throw new NullReferenceException("The stream must be initilized.");

			theStreamer.Volume = (float)Volume / 100;
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
					theStreamer.OnBufferChange += (i) => {
						mainModel.BufferLen = i;
					};
					theStreamer.OnStreamTitleChange += (t, a) => {
						this.Dispatcher.Invoke(new Action(() => {
							//mainModel.StreamTitle = t;

							var payload = new Dictionary<string, object>() {
								{ "action", "radio-info" },
							};

							String playListData = HttpPostRequest.SecureAPICall(payload)["data"].ToString();
							Util.Print(playListData);

							RadioInfo radioInfo = (JsonConvert.DeserializeObject(playListData, typeof(RadioInfo)) as RadioInfo);
							//TODO move me to view model.
							extraModel.Votes = (int)radioInfo.rating;
						}));

					};
					Connected = true;
				} catch (Exception) { }
			}

		}


		private void Window_KeyUp(object sender, KeyEventArgs e) {
			if (e.Key == Key.Escape)
				Application.Current.Shutdown();
		}

		private void Window_Closed(object sender, EventArgs e) {
			theStreamer.Stop();
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


		private void Grid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {

		}


	}
}
