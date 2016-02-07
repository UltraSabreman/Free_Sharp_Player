﻿using System;
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
		public Track CurrentSong { get { return GetProp<Track>(); } set { SetProp(value); } }

		//TODO: set votes to "---" when stopped.


		private MainWindow window;
		private StreamManager streamManager;

		public ExtraMenuModel(MainWindow win, StreamManager man) {
			window = win;
			streamManager = man;

			streamManager.OnEventTrigger += OnEvent;
			streamManager.OnQueueTick += OnTick;
			streamManager.OnBufferingStateChange += OnBufferChange;

			window.Extras.DataContext = this;
			window.ExtrasMenu.Opened += UpdateRating;

			window.btn_Favor.Click += ResetCapture;
			window.btn_Request.Click += ResetCapture;
			window.btn_Settings.Click += ResetCapture;
			window.btn_Request.Click += btn_Request_Click;


			Votes = "---";
		}



		private void btn_Like_Click(object sender, RoutedEventArgs e) {
			CurrentSong.MyVote = (CurrentSong.MyVote == 1 ? -1 : 1);

			int rating = int.Parse(CurrentSong.rating) + CurrentSong.MyVote;
			Votes = rating.ToString();

			setVote.doPost(CurrentSong.MyVote, CurrentSong.trackID);
		}

		private void btn_Dislike_Click(object sender, RoutedEventArgs e) {
			CurrentSong.MyVote += (CurrentSong.MyVote - 1 < -1 ? 1 : -1);

			int rating = int.Parse(CurrentSong.rating) + CurrentSong.MyVote;
			Votes = rating.ToString();

			setVote.doPost(CurrentSong.MyVote, CurrentSong.trackID);

		}

		public void OnEvent(EventTuple ev) {
			if (ev.Event != EventType.Disconnect)
				CurrentSong = ev.CurrentSong;
		}

		public void OnTick(QueueSettingsTuple set) { } 

		public void OnBufferChange(bool isBuffering) { }

		public void UpdateRating(Object o, EventArgs e) {
			getRadioInfo info = getRadioInfo.doPost();

			if (CurrentSong == null || String.IsNullOrEmpty(CurrentSong.trackID) || CurrentSong.trackID == "0") {
				Votes = "---";
				return;
			}

			Track tempTrack = getTrack.GetSingleTrack(CurrentSong.trackID);
			if (tempTrack == null) return;

			String rating = tempTrack.rating;

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
