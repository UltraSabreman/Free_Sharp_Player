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

		private bool VolumeOpen = false;
		private bool ExtrasOpen = false;
		MouseButtonEventHandler VolumeOutClick;
		MouseButtonEventHandler ExtrasOutClick;

		public StreamManager streamManager;

		public MainModel(MainWindow win) {
			window = win;


			window.MyBuffer.DataContext = this;

			window.btn_PlayPause.Click += btn_PlayPause_Click;
			window.btn_Volume.Click += btn_Volume_Click;
			window.btn_Extra.Click += btn_Extra_Click;
			window.btn_Like.Click += btn_Like_Click;
			window.btn_Dislike.Click += btn_Dislike_Click;

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
		}

		public void ConnectThread() {
			ConnectToStream(StreamQuality.Normal);


			window.Dispatcher.Invoke(new Action(() => {
				MaxBufferSize = streamManager.MaxBufferSize;
			}));

			streamManager.UpdateData(null);

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

		public void UpdateSong(Track song) {
			new Thread(() => {
				currentSong = song;
				window.Dispatcher.Invoke(new Action(() => {
					TheTitle = song.WholeTitle;
				}));
				getVoteStatus tempStatus = getVoteStatus.doPost();
				//TODO: make this not rely on a post.
				currentSong.MyVote = tempStatus.vote != null ? (int)tempStatus.vote : 0;
				ColorLikes(tempStatus.status);
			}).Start();
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
			new Thread(() => {
				isLive = int.Parse(info.autoDJ) == 0;
				if (isLive)
					TheTitle = info.title;
			}).Start();
		}

		//TODO: make sure to fix this.
		public void Tick(Object o, EventArgs e) {
			new Thread(() => {
				lock (theLock) {
					Thread updateEvents = new Thread(() => {
						var list = streamManager.GetEvents();
						window.Dispatcher.Invoke(new Action(() => {
							window.MyBuffer.Update(list);
						}));
					});

					updateEvents.Priority = ThreadPriority.AboveNormal;
					updateEvents.Start();


					if (isLive || currentSong == null || !window.IsPlaying) {
						window.Dispatcher.Invoke(new Action(() => {
							SongLength = -1;
							SongMaxLength = 1;
						}));
						return;
					}

					//TODO: song progress fix timing issue
					TimeSpan SongDuration = TimeSpan.Parse(currentSong.duration);
					TimeSpan duration = DateTime.Now - currentSong.localLastPlayed;

					window.Dispatcher.Invoke(new Action(() => {
						if (TotalBufferSize <= 0.5 || PlayedBufferSize <= 0.5)
							Util.PrintLine(TotalBufferSize + " " + PlayedBufferSize);

						TotalBufferSize = streamManager.TotalLength;
						PlayedBufferSize = streamManager.PlayedLegnth;

						double length = (duration.TotalSeconds / SongDuration.TotalSeconds) * SongDuration.TotalSeconds;
						//TODO: backwards hack to get around lack of "live" indicator.
						if (length > (SongDuration.TotalSeconds + 5)) {
							SongLength = -1;
							SongMaxLength = 1;
							isLive = true;
						} else {
							SongMaxLength = SongDuration.TotalSeconds;
							SongLength = (duration.TotalSeconds / SongDuration.TotalSeconds) * SongMaxLength;
						}
					}));
				}
			}).Start();
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
		}



	}
}
