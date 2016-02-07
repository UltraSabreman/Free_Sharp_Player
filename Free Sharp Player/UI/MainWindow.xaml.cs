using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Net;
using System.Collections.Specialized;
using System.Threading;

namespace Free_Sharp_Player {
    using Timer = System.Timers.Timer;


    public partial class MainWindow : Window {
		private Object trackLock = new Object();
		private Object radioLock = new Object();

		[DllImport("kernel32")]
		static extern bool AllocConsole();

		public enum StreamQuality {Low, Normal, High};
		public bool IsPlaying { get; private set; }

		private Timer doubleClickCheck = new Timer(350);
		private bool isDoubleClicking = false;

        Timer MainClock = new Timer(1000);

        MusicStream theQueue;

        private getRadioInfo radioInfo;
		private MainModel mainModel;
		private VolumeModel volumeModel;
		private ExtraMenuModel extraModel;
		private PlaylistModel playlistModel;

		public MainWindow() {
			InitializeComponent();
            btn_PlayPause.IsEnabled = false;

            AllocConsole();

            doubleClickCheck.Elapsed += (o, e) => {
				isDoubleClicking = false;
			};

            mainModel = new MainModel(this);
			volumeModel = new VolumeModel(this);
			extraModel = new ExtraMenuModel(this);
			playlistModel = new PlaylistModel(this);


            MainClock.AutoReset = true;
            MainClock.Elapsed += (o, e) => {
                Update(null);
            };
            MainClock.Start();

            //TODO: UI alert of loading.
            new Thread(() => {
				ConnectToStream(StreamQuality.Normal);

                MainClock.Elapsed += mainModel.Tick;
                MainClock.Elapsed += volumeModel.Tick;
                MainClock.Elapsed += extraModel.Tick;
                MainClock.Elapsed += playlistModel.Tick;

                //Needed?
                //Update(null);

                Dispatcher.Invoke(new Action(() => {
					btn_PlayPause.IsEnabled = true;
				}));
			}).Start();
		}


		public void Play() { IsPlaying = true; theQueue.Play(); }
		public void Stop() { IsPlaying = false; theQueue.Stop(); }

		public void SetVolume(double Volume) {
			if (Volume < 0 || Volume > 100) throw new ArgumentOutOfRangeException("Volume", Volume, "Volume must be between 0 and 100");

			if (theQueue != null)
                theQueue.Volume = (float)Volume / 100;
		}

        private void Update(MusicStream.EventTuple e) {
            Thread.Sleep(2000);
            radioInfo = getRadioInfo.doPost();

            if (e != null) {
                if (e.Event == EventType.SongChange) {
                    List<lastPlayed> playedList = lastPlayed.doPost();

                    Track currentTrack = getTrack.doPost(int.Parse(playedList.First().trackID)).track[0];
                    currentTrack.localLastPlayed = DateTime.Now;

                    lock (trackLock) {
                        mainModel.UpdateSong(currentTrack);
                        playlistModel.UpdateSong(currentTrack);
                    }
                } else if (e.Event == EventType.StateChange) {
                    //The only time we're reciving this event is when the state changes
                    //to Buffering and back to Playing.

                    if (e.State == StreamState.Buffering) {

                    } else {

                    }
                }
            }

            lock (radioLock) {
                mainModel.UpdateInfo(radioInfo);
                extraModel.UpdateInfo(radioInfo);
                playlistModel.UpdateInfo(radioInfo);
            }
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


				try {
                    theQueue = new MusicStream(address, Convert.ToInt32(Configs.Get("MaxBufferLenSec")), Convert.ToInt32(Configs.Get("MaxTotalBufferdSongSec"))
                        , Convert.ToInt32(Configs.Get("MinBufferLenSec")), Convert.ToDouble(Configs.Get("Volume")));

                    theQueue.OnStreamEvent += (MusicStream.EventTuple e) => {
                        Update(e);
                    };

					Connected = true;
				} catch (Exception e) {
                    Console.WriteLine(e.Message);
                }
			}

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
