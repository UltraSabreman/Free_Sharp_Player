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
using Streamer;
using System.Diagnostics;
using System.Timers;
using Newtonsoft.Json;
using System.Windows.Media.Animation;
using Plugin_Base;

/*
 * TODO: 
 * Make sure nothig is taking up UI thread for requests
 * Move ALL logic out of models and into controllers
 * Decouple the shit out of it so nothing needs access to the Queues variables
 * Make sure api calls can only happen when applicable! (IE: can only vote on currently playing song)
 * Fix song dealys
 * Fix starting/stopping stream 
*/
namespace Free_Sharp_Player {
	using Timer = System.Timers.Timer;


	public partial class MainWindow : Window {
		private Object trackLock = new Object();
		private Object radioLock = new Object();

		public bool IsPlaying { get; private set; }

		private Timer doubleClickCheck = new Timer(350);
		private bool isDoubleClicking = false;

		//Timer Updater = new Timer(1000);

		private MusicStream theQueue;

		private MainModel mainModel;
		private VolumeModel volumeModel;
		private ExtraMenuModel extraModel;
		private PlaylistModel playlistModel;

		public MainWindow() {
			InitializeComponent();
			btn_PlayPause.IsEnabled = false;

			doubleClickCheck.Elapsed += (o, e) => {
				isDoubleClicking = false;
			};

			new Thread(() => {
				ConnectToStream(StreamQuality.Normal);
				//streamManager.UpdateData(null);

				Dispatcher.Invoke(new Action(() => {
					mainModel = new MainModel(this, theQueue);
					volumeModel = new VolumeModel(this);
					extraModel = new ExtraMenuModel(this, theQueue);
					playlistModel = new PlaylistModel(this);

					btn_PlayPause.IsEnabled = true;
					Connecting.Visibility = System.Windows.Visibility.Collapsed;
				}));

			}).Start();
		}


		private void ConnectToStream(StreamQuality Quality) {
			String address = "";
			bool Connected = false;

			while (!Connected) {
                var temp = App.Plugins.First().GetStreamInformation(Plugin_Base.Quality.Medium);
                address = temp.StreamAddress;

                theQueue = new MusicStream(address, Convert.ToInt32(Configs.Get("MaxBufferLenSec")), Convert.ToInt32(Configs.Get("MaxTotalBufferdSongSec"))
                    , Convert.ToInt32(Configs.Get("MinBufferLenSec")), Convert.ToDouble(Configs.Get("Volume")));

                theQueue.OnStreamEvent += (MusicStream.EventTuple e) => {
                    Update(e);
                };

                Connected = true;
			}

		}

        private void Update(MusicStream.EventTuple e) {
            Thread.Sleep(2000);
            //TODO: Integrate Plugin
            //getRadioInfo radioInfo = getRadioInfo.doPost();

            if (e != null) {
                if (e.Event == EventType.SongChange) {
                    //TODO: Integrate Plugin
                    /*List<Track> playedList = getLastPlayed.doPost();

                    Track currentTrack = getTrack.doPost(playedList.First().trackID).track[0];
                    //currentTrack.localLastPlayed = DateTime.Now;

                    lock (trackLock) {
                        extraModel.UpdateSong(currentTrack);
                        mainModel.UpdateSong(currentTrack);
                    }*/
                } else if (e.Event == EventType.StateChange) {
                    //The only time we're reciving this event is when the state changes
                    //to Buffering and back to Playing.

                    if (e.State == StreamState.Buffering) {

                    } else {

                    }
                }
            }

            lock (radioLock) {
                //TODO: Integrate Plugin
                //mainModel.UpdateInfo(radioInfo);
            }

            mainModel.OnTick();
        }

        //TODO: run on event

        public void MainTick(Object o, EventArgs e) {
			new Thread(() => { }).Start();

			//TODO: main song tick

			//mainModel.UpdateSongProgress(playlistModel.Playing, playlistModel.Played[0], theStreamer.startTime, radioInfo);
		}


		public void Play() { IsPlaying = true; theQueue.Play(); }
		public void Stop() { 
			IsPlaying = false;
            theQueue.Stop();
            //What is this.
			Dispatcher.Invoke(new Action(() => {
				MyBuffer.Update(null);
			}));
		}

		public void SetVolume(double Volume) {
			if (Volume < 0 || Volume > 100) throw new ArgumentOutOfRangeException("Volume", Volume, "Volume must be between 0 and 100");

			if (theQueue != null)
                theQueue.Volume = (float)Volume / 100;
		}



		private void Grid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
			//currentSong.trackID;
		}
		private void Window_KeyUp(object sender, KeyEventArgs e) {
			if (e.Key == Key.Escape)
				Application.Current.Shutdown();
		}

		private void Window_Closed(object sender, EventArgs e) {
            theQueue.Stop();
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
