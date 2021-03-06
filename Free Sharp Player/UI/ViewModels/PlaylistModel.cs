﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using Plugin_Base;

namespace Free_Sharp_Player {
    using Timer = System.Timers.Timer;

    class PlaylistModel : ViewModelNotifier {
		private Object theLock = new Object();
	
		public ObservableCollection<Track> Played { get { return GetProp<ObservableCollection<Track>>(); } set { SetProp(value); } }
		public ObservableCollection<Track> Queue { get { return GetProp<ObservableCollection<Track>>(); } set { SetProp(value); } }
		public bool IsOpen { get; private set; }

		private double MaxQueueHight = 0;
		private double MaxPlayedHight = 0;

		private MainWindow window;
	
		private Timer listUpdater = new Timer(10000);

		public PlaylistModel(MainWindow win) {
			window = win;

			Played = new ObservableCollection<Track>();
			Queue = new ObservableCollection<Track>();

  
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

		public void Tick(Object o = null, EventArgs e = null) {
			new Thread(() => {
                //TODO: Integrate Plugin
                /*var played = getLastPlayed.doPost();
				var reqs = getRequests.doPost();
				var queued = new List<Track>();
				if (reqs != null && reqs.number_of_tracks > 0)
					queued = reqs.track;

				window.Dispatcher.Invoke(new Action(() => {
					
						UpdatePlayedList(played);

						UpdateQueue(queued);

						UpdateSize();
				}));*/
            }).Start();
		}

		//TODO: new tracks added in bottom(needs to be on top)
		private void UpdatePlayedList(List<Track> newStuff) {
			List<Track> toRemove = new List<Track>();

			if (newStuff == null) return;
			bool flag = Played.Count == 0;

			int index = 0;
			foreach (Track lp in newStuff) {
				var matches = Played.Where(X => X.TrackID == lp.TrackID);
				if (matches.Count() == 0) {
					if (flag)
						Played.Add(lp);
					else
						Played.Insert(index++, lp);
				}
                //TODO: Integrate Plugin
                //else
                //matches.ElementAt(0).Update(lp);
            }

            foreach (Track lp in Played) {
                Track match = newStuff.Find(X => X.TrackID == lp.TrackID);
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

			int index = 0;
			foreach (Track lp in newStuff) {
				var matches = Queue.Where(X => X.TrackID == lp.TrackID);
				if (matches.Count() == 0) {
					if (flag)
						Queue.Add(lp);
					else
						Queue.Insert(index++, lp);
				}
                //TODO: Integrate Plugin
                //else
                //matches.ElementAt(0).Update(lp);
            }

            foreach (Track lp in Queue) {
				Track match = newStuff.Find(X => X.TrackID == lp.TrackID);
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