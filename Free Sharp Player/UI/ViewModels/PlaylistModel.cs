using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Free_Sharp_Player {
	class PlaylistModel : ViewModelNotifier {
		private Object theLock = new Object();
	
		public String StreamTitle { get { return GetProp<String>(); } set { SetProp(value); } }
		public ObservableCollection<getLastPlayed> Played { get { return GetProp<ObservableCollection<getLastPlayed>>(); } set { SetProp(value); } }
		public ObservableCollection<Track> Queue { get { return GetProp<ObservableCollection<Track>>(); } set { SetProp(value); } }
		public Track Playing { get { return GetProp<Track>(); } set { SetProp(value); } }
		public bool IsOpen { get; private set; }

		private double MaxQueueHight;
		private double MaxPlayedHight;

		MainWindow window;
		private Timer listUpdater = new Timer(10000);

		public PlaylistModel(MainWindow win) {
			Played = new ObservableCollection<getLastPlayed>();
			Queue = new ObservableCollection<Track>();

			SetWindow(win);
		}

		public void SetWindow(MainWindow win) {
			window = win;

			window.QueueList.DataContext = this;
			window.PlayedList.DataContext = this;


			Tick(null, null);

			MaxQueueHight = 0;
			MaxPlayedHight = 0;


			window.LocationChanged += (o, e) => {
				window.Played.HorizontalOffset += 1;
				window.Played.HorizontalOffset -= 1;

				window.Queue.HorizontalOffset += 1;
				window.Queue.HorizontalOffset -= 1;
			};

			window.Deactivated += (o, e) => {
				window.Queue.IsOpen = false;
				window.Played.IsOpen = false;
				IsOpen = false;
			};

			listUpdater.Elapsed += Tick;
			listUpdater.AutoReset = true;
			listUpdater.Start();

			StreamTitle = "Not Connected";
		}

		public void Tick(Object o, EventArgs e) {
			//TODO: playlist and other bs

		}

		public void UpdateLists(List<getLastPlayed> played, List<Track> queued) {
			window.Dispatcher.Invoke(new Action(() => {
				lock (theLock) {
					UpdatePlayedList(played);

					UpdateQueue(queued);

					UpdateSize();
				}
			}));
		}

		private void UpdatePlayedList(List<getLastPlayed> newStuff) {
			List<getLastPlayed> toRemove = new List<getLastPlayed>();

			if (newStuff == null) return;

			foreach (getLastPlayed lp in newStuff) {
				var matches = Played.Where(X => X.trackID == lp.trackID);
				if (matches.Count() == 0)
					Played.Add(lp);
				else
					matches.ElementAt(0).Update(lp);
			}

			foreach (getLastPlayed lp in Played) {
				getLastPlayed match = newStuff.Find(X => X.trackID == lp.trackID);
				if (match == null)
					toRemove.Add(lp);
			}

			toRemove.ForEach(X => Played.Remove(X));
		}

		//TODO: eliminate duplicate code.
		private void UpdateQueue(List<Track> newStuff) {
			List<Track> toRemove = new List<Track>();

			if (newStuff == null) return;

			foreach (Track lp in newStuff) {
				var matches = Queue.Where(X => X.trackID == lp.trackID);
				if (matches.Count() == 0)
					Queue.Add(lp);
				else
					matches.ElementAt(0).Update(lp);
			}

			foreach (Track lp in Queue) {
				Track match = newStuff.Find(X => X.trackID == lp.trackID);
				if (match == null)
					toRemove.Add(lp);
			}

			toRemove.ForEach(X => Queue.Remove(X));
		}

		public void UpdateSong(Track song) {
			Playing = song;
		}

		public void UpdateInfo(getRadioInfo info) {
			if (!window.IsPlaying)
				StreamTitle = "Not Connected";
			else
				StreamTitle = info.title;
		}


		public void UpdateSize() {
			if (Queue != null && Queue.Count > 0)
				MaxQueueHight = (Queue.Count * 15);
			else
				MaxQueueHight = 0;

			if (Played != null && Played.Count > 0)
				MaxPlayedHight = (Played.Count * 15);
			else
				MaxPlayedHight = 0;

			window.Dispatcher.Invoke(new Action(() => {
				window.Played.Height = MaxPlayedHight;
				window.Queue.Height = MaxQueueHight;
			}));
		}

		public void AnimateLists() {
			IsOpen = !IsOpen;
			window.Played.IsOpen = IsOpen;
			window.Queue.IsOpen = IsOpen;
			
		}


	}
}