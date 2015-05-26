using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Free_Sharp_Player {
	using Timer = System.Timers.Timer;

	class PlaylistModel : ViewModelNotifier {
		private Object theLock = new Object();
	
		public ObservableCollection<getLastPlayed> Played { get { return GetProp<ObservableCollection<getLastPlayed>>(); } set { SetProp(value); } }
		public ObservableCollection<Track> Queue { get { return GetProp<ObservableCollection<Track>>(); } set { SetProp(value); } }
		public bool IsOpen { get; private set; }

		private double MaxQueueHight = 0;
		private double MaxPlayedHight = 0;

		private MainWindow window;
		private StreamManager streamManager;

		private Timer listUpdater = new Timer(10000);

		public PlaylistModel(MainWindow win, StreamManager man) {
			window = win;
			streamManager = man;

			Played = new ObservableCollection<getLastPlayed>();
			Queue = new ObservableCollection<Track>();

			streamManager.OnEventTrigger += OnEvent;
			streamManager.OnQueueTick += OnTick;
			streamManager.OnBufferingStateChange += OnBufferChange;


			window.QueueList.DataContext = this;
			window.PlayedList.DataContext = this;

			//Move popups with window
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

			Tick();
		}

		public void OnEvent(EventTuple ev) { }

		public void OnTick(QueueSettingsTuple set) { }

		public void OnBufferChange(bool isBuffering) { }


		public void Tick(Object o = null, EventArgs e = null) {
			new Thread(() => {
				var played = getLastPlayed.doPost();
				var reqs = getRequests.doPost();
				var queued = new List<Track>();
				if (reqs != null && reqs.number_of_tracks > 0)
					queued = reqs.track;

				window.Dispatcher.Invoke(new Action(() => {
					
						UpdatePlayedList(played);

						UpdateQueue(queued);

						UpdateSize();

						Util.PrintLine("asdfadfasdf");
					
				}));
			}).Start();
		}

		//TODO: new tracks added in bottom(needs to be on top)
		private void UpdatePlayedList(List<getLastPlayed> newStuff) {
			List<getLastPlayed> toRemove = new List<getLastPlayed>();

			if (newStuff == null) return;
			bool flag = Played.Count == 0;


			foreach (getLastPlayed lp in newStuff) {
				var matches = Played.Where(X => X.trackID == lp.trackID);
				if (matches.Count() == 0) {
					if (flag)
						Played.Add(lp);
					else
						Played.Insert(0, lp);
				} else
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
			bool flag = Queue.Count == 0;

			foreach (Track lp in newStuff) {
				var matches = Queue.Where(X => X.trackID == lp.trackID);
				if (matches.Count() == 0) {
					if (flag)
						Queue.Add(lp);
					else
						Queue.Insert(0, lp);
				} else
					matches.ElementAt(0).Update(lp);
			}

			foreach (Track lp in Queue) {
				Track match = newStuff.Find(X => X.trackID == lp.trackID);
				if (match == null)
					toRemove.Add(lp);
			}

			toRemove.ForEach(X => Queue.Remove(X));
		}

		/*public void UpdateInfo(getRadioInfo info) {
			window.Dispatcher.Invoke(new Action(() => {
				if (!window.IsPlaying)
					StreamTitle = "Not Connected";
				else
					StreamTitle = info.title;
			}));
		}*/


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
			listUpdater.Enabled = IsOpen;
			if (IsOpen)
				Tick();
		}


	}
}