using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Free_Sharp_Player {
    using Timer = System.Timers.Timer;

	public class MainModel : ViewModelNotifier {
		private Object theLock = new Object();


		public double MaxBufferSize { get { return GetProp<double>(); } set { SetProp(value); } }
		public double TotalBufferSize { get { return GetProp<double>(); } set { SetProp(value); } }
		public double PlayedBufferSize { get { return GetProp<double>(); } set { SetProp(value); } }
		public double SongMaxLength { get { return GetProp<double>(); } set { SetProp(value); } }
		public double SongLength { get { return GetProp<double>(); } set { SetProp(value); } }
		public String TheTitle { get { return GetProp<String>(); } set { SetProp(value); } }

		private Track currentSong;
		private bool isLive;

		private MainWindow window;
		private MusicStream streamManager;

		private bool VolumeOpen = false;
		private bool ExtrasOpen = false;
		MouseButtonEventHandler VolumeOutClick;
		MouseButtonEventHandler ExtrasOutClick;

        Timer ticker = new Timer(100);

        public MainModel(MainWindow win, MusicStream manger) {
			window = win;
			streamManager = manger;

			window.MyBuffer.DataContext = this;

			window.btn_PlayPause.Click += btn_PlayPause_Click;
			window.btn_Volume.Click += btn_Volume_Click;
			window.btn_Extra.Click += btn_Extra_Click;

            ticker.AutoReset = true;
            ticker.Elapsed += (o, e) => {
                OnTick();
            };
            ticker.Start();


            window.MyBuffer.OnSeekDone += (sec) => {
				PlayedBufferSize += sec;
				if (streamManager != null)
					streamManager.Seek(sec);
			};

			VolumeOutClick = new MouseButtonEventHandler(HandleClickOutsideOfVolume);
			ExtrasOutClick = new MouseButtonEventHandler(HandleClickOutsideOfExtras);

			window.UpdateLayout();

			currentSong = null;
			isLive = false;
			TheTitle = "Not Connected";
		}

		public void UpdateSong(Track song) {
			if (song == null) return;
            new Thread(() => {

                currentSong = song;
                if (currentSong != null) {
                    window.Dispatcher.Invoke(new Action(() => {
                        TheTitle = currentSong.WholeTitle;
                    }));


                    getVoteStatus tempStatus = getVoteStatus.doPost();
                    if (currentSong != null) {
                        currentSong.MyVote = (int)(tempStatus.vote ?? 0);
                    }
                }
            }).Start();

		}

        public void UpdateInfo(getRadioInfo info) {
            if (info != null)
                isLive = int.Parse(info.autoDJ) == 0;
            else
                isLive = false;
        }

		public void OnTick() {
			new Thread(() => {
				lock (theLock) {
					MaxBufferSize = streamManager.MaxTimeInQueue;


					//Update event list. 
					new Thread(() => {
						var list = streamManager.GetAllEvents();
						window.Dispatcher.Invoke(new Action(() => {
							window.MyBuffer.Update(list);
						}));
					}).Start();


					//TODO: more colors/states.
					if (isLive || currentSong == null || !window.IsPlaying || currentSong.duration == null) {
						window.Dispatcher.Invoke(new Action(() => {
							SongLength = -1;
							SongMaxLength = 1;

							TotalBufferSize = streamManager.TotalTimeInQueue;
							PlayedBufferSize = streamManager.TotalTimeInQueue - streamManager.BufferedTime;
						}));
						return;
					}
					
					//TODO: song progress fix timing issue
					TimeSpan SongDuration = TimeSpan.Parse(currentSong.duration);
					TimeSpan duration = DateTime.Now - (currentSong.localLastPlayed + new TimeSpan(0,0,0, (int)streamManager.BufferedTime, 0));

					window.Dispatcher.Invoke(new Action(() => {
						if (TotalBufferSize <= 0.5 || PlayedBufferSize <= 0.5)
							Util.PrintLine(TotalBufferSize + " " + PlayedBufferSize);

						TotalBufferSize = streamManager.TotalTimeInQueue;
						PlayedBufferSize = streamManager.TotalTimeInQueue - streamManager.BufferedTime;

						double length = (duration.TotalSeconds / SongDuration.TotalSeconds) * SongDuration.TotalSeconds;
						//TODO: backwards hack to get around lack of "live" indicator.
						/*if (length > (SongDuration.TotalSeconds + 5)) {
							SongLength = -1;
							SongMaxLength = 1;
							isLive = true;
						} else {*/
							SongMaxLength = SongDuration.TotalSeconds;
							SongLength = (duration.TotalSeconds / SongDuration.TotalSeconds) * SongMaxLength;
						//}
					}));
				}
			}).Start();
		}

		public void OnBufferChange(bool isBuffering) {

		}


		/*public void UpdateInfo(getRadioInfo info) {
			new Thread(() => {
				isLive = int.Parse(info.autoDJ) == 0;
				if (isLive)
					TheTitle = info.title;
			}).Start();
		}*/

		private void btn_PlayPause_Click(object sender, RoutedEventArgs e) {
			if (window.IsPlaying) {
				window.Stop();
				window.btn_PlayPause.Content = "▶";
			} else {
				window.Play();
				window.btn_PlayPause.Content = "■";
			}
		}

		private void HandleClickOutsideOfVolume(object sender, MouseButtonEventArgs e) {
			Console.WriteLine("OutVolClick");
			window.Volume.ReleaseMouseCapture();
			window.Volume.RemoveHandler(Mouse.PreviewMouseDownOutsideCapturedElementEvent, VolumeOutClick);


			if (!window.btn_Volume.IsMouseOver && VolumeOpen)
				btn_Volume_Click(null, null);
		}

		private void HandleClickOutsideOfExtras(object sender, MouseButtonEventArgs e) {
			Console.WriteLine("OutExtClick");
			window.Extras.ReleaseMouseCapture();
			window.Extras.RemoveHandler(Mouse.PreviewMouseDownOutsideCapturedElementEvent, ExtrasOutClick);

			if (!window.btn_Extra.IsMouseOver && ExtrasOpen)
				btn_Extra_Click(null, null);
		}

		private void btn_Volume_Click(object sender, RoutedEventArgs e) {
			VolumeOpen = !VolumeOpen;
			window.VolumeMenu.IsOpen = VolumeOpen;
			if (VolumeOpen) {
				Mouse.Capture(window.Volume, CaptureMode.SubTree);
				window.Volume.AddHandler(Mouse.PreviewMouseDownOutsideCapturedElementEvent, VolumeOutClick, true);
			}
		}



		private void btn_Extra_Click(object sender, RoutedEventArgs e) {
			ExtrasOpen = !ExtrasOpen;
			window.ExtrasMenu.IsOpen = ExtrasOpen;
			if (ExtrasOpen) {
				Mouse.Capture(window.Extras, CaptureMode.SubTree);
				window.Extras.AddHandler(Mouse.PreviewMouseDownOutsideCapturedElementEvent, ExtrasOutClick, true);
			}
		}



	}
}
