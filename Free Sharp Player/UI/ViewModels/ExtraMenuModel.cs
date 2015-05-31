using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Free_Sharp_Player {
	class ExtraMenuModel : ViewModelNotifier {
		public String Votes { get { return GetProp<String>(); } set { SetProp(value); } }

		//TODO: set votes to "---" when stopped.


		private MainWindow window;
		private StreamManager streamManager;
		private int updateCount = 0;

		private Track currentSong;

		public ExtraMenuModel(MainWindow win, StreamManager man) {
			window = win;
			streamManager = man;

			streamManager.OnEventTrigger += OnEvent;
			streamManager.OnQueueTick += OnTick;
			streamManager.OnBufferingStateChange += OnBufferChange;

			window.Extras.DataContext = this;
			window.ExtrasMenu.Opened += UpdateRating;

			window.btn_Like.Click += btn_Like_Click;
			window.btn_Like.Click += ResetCapture;
			window.btn_Dislike.Click += btn_Dislike_Click;
			window.btn_Dislike.Click += ResetCapture;

			window.btn_Favor.Click += ResetCapture;
			window.btn_Request.Click += ResetCapture;
			window.btn_Settings.Click += ResetCapture;
			window.btn_Request.Click += btn_Request_Click;


			Votes = "---";
		}



		private void ColorLikes(int? status = null) {
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


		private void btn_Like_Click(object sender, RoutedEventArgs e) {
			currentSong.MyVote = (currentSong.MyVote == 1 ? -1 : 1);

			int rating = int.Parse(currentSong.rating) + currentSong.MyVote;
			Votes = rating.ToString();

			ColorLikes();
			setVote.doPost(currentSong.MyVote, currentSong.trackID);
		}

		private void btn_Dislike_Click(object sender, RoutedEventArgs e) {
			currentSong.MyVote += (currentSong.MyVote - 1 < -1 ? 1 : -1);

			int rating = int.Parse(currentSong.rating) + currentSong.MyVote;
			Votes = rating.ToString();

			ColorLikes();
			setVote.doPost(currentSong.MyVote, currentSong.trackID);

		}

		public void OnEvent(EventTuple ev) {
			if (ev.Event != EventType.Disconnect)
				currentSong = ev.CurrentSong;
		}

		public void OnTick(QueueSettingsTuple set) { } 

		public void OnBufferChange(bool isBuffering) { }

		public void UpdateRating(Object o, EventArgs e) {
			getRadioInfo info = getRadioInfo.doPost();
			if (info.autoDJ != "1")
				ColorLikes(0);
			else
				ColorLikes();

			if (currentSong == null || String.IsNullOrEmpty(currentSong.trackID) || currentSong.trackID == "0") {
				Votes = "---";
				return;
			}

			var trackList = getTrack.doPost((int?)int.Parse(currentSong.trackID));
			if (trackList == null || trackList.total_records == "0") 
				return;

			String rating = trackList.track.First().rating;

			window.Dispatcher.Invoke(new Action(() => {
				Votes = rating;
			}));
		}

		private void btn_Request_Click(object sender, RoutedEventArgs e) {
			BasicRequestUI ui = new BasicRequestUI();
			ui.Show();
		}


		private void ResetCapture(object o, object e) {
			Mouse.Capture(window.Extras, CaptureMode.SubTree);
		}
	}
}
