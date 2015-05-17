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

			btn_PlayPause.IsEnabled = false;
			//TODO: UI alert of loading.
			new Thread(() => {
				mainModel.ConnectThread();

				mainModel.streamManager.NewCurrentTrack += (Track track) => {
					lock (trackLock) {
						mainModel.UpdateSong(track);
						playlistModel.UpdateSong(track);
					}
				};

				mainModel.streamManager.OnRadioUpdate += (getRadioInfo info, List<getLastPlayed> played, List<Track> queued) => {
					lock (radioLock) {
						mainModel.UpdateInfo(info);
						playlistModel.UpdateLists(played, queued);
						extraModel.UpdateInfo(info);
						playlistModel.UpdateInfo(info);

					}
				};

				mainModel.streamManager.mainUpdateTimer.Elapsed += mainModel.Tick;
				mainModel.streamManager.mainUpdateTimer.Elapsed += volumeModel.Tick;
				mainModel.streamManager.mainUpdateTimer.Elapsed += extraModel.Tick;
				mainModel.streamManager.mainUpdateTimer.Elapsed += playlistModel.Tick;
				mainModel.streamManager.mainUpdateTimer.Elapsed += MainTick;

				Dispatcher.Invoke(new Action(() => {
					btn_PlayPause.IsEnabled = true;
					Connecting.Visibility = System.Windows.Visibility.Collapsed;
				}));
			}).Start();
		}

		public void MainTick(Object o, EventArgs e) {
			new Thread(() => { }).Start();

			//TODO: main song tick

			//mainModel.UpdateSongProgress(playlistModel.Playing, playlistModel.Played[0], theStreamer.startTime, radioInfo);
		}


		public void Play() { IsPlaying = true; mainModel.streamManager.Play(); }
		public void Stop() { 
			IsPlaying = false; 
			mainModel.streamManager.Stop();
			Dispatcher.Invoke(new Action(() => {
				MyBuffer.Update(null);
			}));
		}

		public void SetVolume(double Volume) {
			if (Volume < 0 || Volume > 100) throw new ArgumentOutOfRangeException("Volume", Volume, "Volume must be between 0 and 100");

			if (mainModel.streamManager != null)
				mainModel.streamManager.Volume = (float)Volume / 100;
		}



		private void Grid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
			//currentSong.trackID;
		}
		private void Window_KeyUp(object sender, KeyEventArgs e) {
			if (e.Key == Key.Escape)
				Application.Current.Shutdown();
		}

		private void Window_Closed(object sender, EventArgs e) {
			mainModel.streamManager.Stop();
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
