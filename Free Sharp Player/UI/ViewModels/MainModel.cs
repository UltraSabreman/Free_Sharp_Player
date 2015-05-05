using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Free_Sharp_Player {
	public class MainModel : ViewModelNotifier {
		private Object theLock = new Object();

		public int PosInBuffer { get { return GetProp<int>(); } set { SetProp(value); } }
		public int BufferLen { get { return GetProp<int>(); } set { SetProp(value); } }
		public int SongLength { get { return GetProp<int>(); } set { SetProp(value); } }
		public int SongProgress { get { return GetProp<int>(); } set { SetProp(value); } }
		public String SongTimeColor { get { return GetProp<String>(); } set { SetProp(value); } }
		public String SongProgressText { get { return GetProp<String>(); } set { SetProp(value); } }

		private Track currentSong;
		private bool isLive;

		private MainWindow window;

		private bool VolumeOpen = false;
		private bool ExtrasOpen = false;
		MouseButtonEventHandler VolumeOutClick;
		MouseButtonEventHandler ExtrasOutClick;


		//TODO: add to configs.
		private String liveColor = "#22cccc";
		private String notLiveColor = "#ff5555";

		//double click on thing to open playlist

		public MainModel(MainWindow win) {
			window = win;


			window.bar_Buffer.DataContext = this;
			window.bar_BufferWindow.DataContext = this;
			window.bar_SongTime.DataContext = this;

			window.btn_PlayPause.Click += btn_PlayPause_Click;
			window.btn_Volume.Click += btn_Volume_Click;
			window.btn_Extra.Click += btn_Extra_Click;
			window.btn_Like.Click += btn_Like_Click;
			window.btn_Dislike.Click += btn_Dislike_Click;


			VolumeOutClick = new MouseButtonEventHandler(HandleClickOutsideOfVolume);
			ExtrasOutClick = new MouseButtonEventHandler(HandleClickOutsideOfExtras);

			window.UpdateLayout();

			currentSong = null;
			isLive = false;
		}


		public void UpdateSong(Track song) {
			currentSong = song;

			getVoteStatus tempStatus = getVoteStatus.doPost();
			//TODO: make this not rely on a post.
			currentSong.MyVote = tempStatus.vote != null ? (int)tempStatus.vote : 0;
			ColorLikes(tempStatus.status);
		}

		private void ColorLikes(int? status) {
			window.Dispatcher.Invoke(new Action(() => {
				if (status != null && status == 0) {
					window.btn_Like.IsEnabled = false;
					window.btn_Dislike.IsEnabled = false;
				} else {
					window.btn_Like.IsEnabled = true;
					window.btn_Dislike.IsEnabled = true;

					if (currentSong.MyVote == 1) {
						window.btn_Like.Background = Brushes.DarkGreen;
						window.btn_Like.Foreground = Brushes.Black;

						window.btn_Dislike.Background = Brushes.Transparent;
						window.btn_Dislike.Foreground = Brushes.DarkRed;
					} else if (currentSong.MyVote == -1) {
						window.btn_Like.Background = Brushes.Transparent;
						window.btn_Like.Foreground = Brushes.DarkGreen;

						window.btn_Dislike.Background = Brushes.DarkRed;
						window.btn_Dislike.Foreground = Brushes.Black;
					} else {
						window.btn_Like.Background = Brushes.Transparent;
						window.btn_Like.Foreground = Brushes.DarkGreen;

						window.btn_Dislike.Background = Brushes.Transparent;
						window.btn_Dislike.Foreground = Brushes.DarkRed;
					}
				}
			}));

		}

		public void UpdateInfo(getRadioInfo info) {
			isLive = int.Parse(info.autoDJ) == 0;
		}

		public void Tick(Object o, EventArgs e) {
			lock (theLock) {
				if (isLive || currentSong == null || !window.IsPlaying) {
					SongTimeColor = notLiveColor;
					SongProgress = 100;
					return;
				} else
					SongTimeColor = liveColor;

				DateTime lastPlayedDate;
				if (currentSong.localLastPlayed != new DateTime(0)) {
					lastPlayedDate = currentSong.localLastPlayed;
				} else {
					TimeZoneInfo hwZone = TimeZoneInfo.Utc;
					lastPlayedDate = TimeZoneInfo.ConvertTime(DateTime.Parse(currentSong.lastPlayed), hwZone, TimeZoneInfo.Local);
				}

				TimeSpan SongDuration = TimeSpan.Parse(currentSong.duration);
				TimeSpan duration = DateTime.Now - lastPlayedDate;
				SongProgress = (int)((duration.TotalSeconds / SongDuration.TotalSeconds) * 100);
				SongProgressText = "Duration: " + duration.TotalSeconds + ", SongDuration: " + (SongDuration.TotalSeconds);
			}
		}


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
			/*DoubleAnimation testan;
			if (VolumeOpen) {
				//Mouse.Capture(window.Volume, CaptureMode.SubTree);
				//window.Volume.AddHandler(Mouse.PreviewMouseDownOutsideCapturedElementEvent, VolumeOutClick, true);

				testan = new DoubleAnimation(0, 120, new Duration(new TimeSpan(0, 0, 0, 0, 100)), FillBehavior.HoldEnd);
				//Util.AnimateWindowMoveY(window, -120);
			} else {
				testan = new DoubleAnimation(120, 0, new Duration(new TimeSpan(0, 0, 0, 0, 100)), FillBehavior.HoldEnd);
				//Util.AnimateWindowMoveY(window, 120);
			}

			Storyboard test = new Storyboard();
			test.Children.Add(testan);
			Storyboard.SetTargetName(testan, window.VolumeMenu.Name);
			Storyboard.SetTargetProperty(testan, new PropertyPath(Grid.HeightProperty));
			test.Begin(window.VolumeMenu);*/

		}



		private void btn_Like_Click(object sender, RoutedEventArgs e) {
			currentSong.MyVote += (currentSong.MyVote + 1 > 1 ? -1 : 1);
			ColorLikes(null);
			setVote.doPost(currentSong.MyVote, currentSong.trackID);
		}
		private void btn_Dislike_Click(object sender, RoutedEventArgs e) {
			currentSong.MyVote += (currentSong.MyVote - 1 < -1 ? 1 : -1);
			ColorLikes(null);
			setVote.doPost(currentSong.MyVote, currentSong.trackID);

		}

		private void btn_Extra_Click(object sender, RoutedEventArgs e) {
			ExtrasOpen = !ExtrasOpen;
			window.ExtrasMenu.IsOpen = ExtrasOpen;
			if (ExtrasOpen) {
				Mouse.Capture(window.Extras, CaptureMode.SubTree);
				window.Extras.AddHandler(Mouse.PreviewMouseDownOutsideCapturedElementEvent, ExtrasOutClick, true);
			}
			/*DoubleAnimation testan;
			if (ExtrasOpen) {
				//Mouse.Capture(window.Extras, CaptureMode.SubTree);
				//window.Extras.AddHandler(Mouse.PreviewMouseDownOutsideCapturedElementEvent, ExtrasOutClick, true);

				testan = new DoubleAnimation(0, 60, new Duration(new TimeSpan(0, 0, 0, 0, 100)), FillBehavior.HoldEnd);
				//Util.AnimateWindowMoveY(window, 60);
			} else {
				testan = new DoubleAnimation(60, 0, new Duration(new TimeSpan(0, 0, 0, 0, 100)), FillBehavior.HoldEnd);
				//Util.AnimateWindowMoveY(window, -60);
			}

			Storyboard test = new Storyboard();
			test.Children.Add(testan);
			Storyboard.SetTargetName(testan, window.ExtrasMenu.Name);
			Storyboard.SetTargetProperty(testan, new PropertyPath(Grid.HeightProperty));
			test.Begin(window.ExtrasMenu);*/
		}



	}
}
