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
using System.Windows.Media.Animation;

namespace Free_Sharp_Player {
	class PlaylistModel : ViewModelNotifier {
		public String StreamTitle { get { return GetProp<String>(); } set { SetProp(value); DoMarquee(); } }
		public ObservableCollection<Track> Played { get { return GetProp<ObservableCollection<Track>>(); } set { SetProp(value); } }
		public ObservableCollection<Track> Queue { get { return GetProp<ObservableCollection<Track>>(); } set { SetProp(value); } }
		public Track Playing { get { return GetProp<Track>(); } set { SetProp(value); } }

		private double MaxQueueHight;
		private double MaxPlayedHight;
		private double MinQueueHeight;
		private double MinPlayedHeight;

		MainWindow window;
		private bool doOpen = false;
		private Timer listUpdater = new Timer(10000);

		public PlaylistModel(MainWindow win) {
			SetWindow(win);
		}

		public PlaylistModel() {

		}

		public void SetWindow(MainWindow win) {
			window = win;

			window.QueueList.DataContext = this;
			window.PlayedList.DataContext = this;
			window.txt_TrackName.DataContext = this;


			Tick(null, null);

			window.QueueList.Margin = new Thickness(0, window.QueueHeight.Height.Value, 0, 0);
			window.PlayedList.Margin = new Thickness(0, 0, 0, window.PlayedHeight.Height.Value);
			MaxQueueHight = MinQueueHeight = window.QueueHeight.Height.Value;
			MaxPlayedHight = MinPlayedHeight = window.PlayedHeight.Height.Value;

			listUpdater.Elapsed += Tick;
			listUpdater.AutoReset = true;
			listUpdater.Start();

		}

		public void Tick(Object o, EventArgs e) {
			//TODO: playlist and other bs

			List<lastPlayed> playedTracks = lastPlayed.doPost();
			List<Track> actualTracks = new List<Track>();

			foreach (lastPlayed l in playedTracks) {
				getTrack temp = getTrack.doPost(l.title, l.artist);
				if (temp != null && temp.track != null && temp.track.Count > 0)
					actualTracks.Add(temp.track.Find(
						X => {
							return Util.trimDateString(X.lastPlayed) == Util.trimDateString(l.last_played);
						}
					));
			}

			getRequests requestedTracks = getRequests.doPost();



			Played = new ObservableCollection<Track>(actualTracks);
			if (requestedTracks.number_of_tracks > 0)
				Queue = new ObservableCollection<Track>(requestedTracks.track);
			else
				Queue = new ObservableCollection<Track>();
			Playing = actualTracks.First();

			StreamTitle = Playing.WholeTitle;
			
			UpdateSize();
		}


		private void DoMarquee() {
			window.Dispatcher.Invoke(new Action(() => {
				DoubleAnimation doubleAnimation = new DoubleAnimation();
				Canvas canvas = window.can_TrackName;
				TextBlock text = window.txt_TrackName;
				TextBlock text2 = window.txt_TrackName2;

				if (canvas != null && text != null && text.ActualWidth >= canvas.ActualWidth) {
					text2.Visibility = Visibility.Visible;
					//todo better marquee math here.
					DoubleAnimation anim = new DoubleAnimation(0, (text.ActualWidth + canvas.ActualWidth / 2), new Duration(new TimeSpan(0, 0, 10)));
					DoubleAnimation anim2 = new DoubleAnimation(-(text.ActualWidth + canvas.ActualWidth / 2), 0, new Duration(new TimeSpan(0, 0, 10)));
					anim.RepeatBehavior = RepeatBehavior.Forever;
					anim2.RepeatBehavior = RepeatBehavior.Forever;

					text.BeginAnimation(Canvas.RightProperty, anim);
					text2.BeginAnimation(Canvas.RightProperty, anim2);
				} else
					text2.Visibility = Visibility.Hidden;
			}));

		}


		public void UpdateSize() {
			if (Queue != null && Queue.Count > 0)
				MaxQueueHight = (Queue.Count * 20);
			else
				MaxQueueHight = MinQueueHeight;
			if (Played != null && Played.Count > 0)
				MaxPlayedHight = (Played.Count * 20);
			else
				MaxPlayedHight = MinPlayedHeight;
		}

		public void AnimateLists() {
			doOpen = !doOpen;
			ThicknessAnimation testan;
			if (doOpen) {
				testan = new ThicknessAnimation(new Thickness(0, MinQueueHeight, 0, 0), new Thickness(0, MaxQueueHight, 0, 0), new Duration(new TimeSpan(0, 0, 0, 0, 100)), FillBehavior.HoldEnd);
			} else {
				testan = new ThicknessAnimation(new Thickness(0, MaxQueueHight, 0, 0), new Thickness(0, MinQueueHeight, 0, 0), new Duration(new TimeSpan(0, 0, 0, 0, 100)), FillBehavior.HoldEnd);
			}

			ThicknessAnimation testan2;
			if (doOpen) {
				testan2 = new ThicknessAnimation(new Thickness(0, 0, 0, MinPlayedHeight), new Thickness(0, 0, 0, MaxPlayedHight), new Duration(new TimeSpan(0, 0, 0, 0, 100)), FillBehavior.HoldEnd);
			} else {
				testan2 = new ThicknessAnimation(new Thickness(0, 0, 0, MaxPlayedHight), new Thickness(0, 0, 0, MinPlayedHeight), new Duration(new TimeSpan(0, 0, 0, 0, 100)), FillBehavior.HoldEnd);
			}

			Storyboard test = new Storyboard();
			test.Children.Add(testan);
			Storyboard.SetTargetName(testan, window.QueueList.Name);
			Storyboard.SetTargetProperty(testan, new PropertyPath(Grid.MarginProperty));
			test.Begin(window.QueueList);

			Storyboard test2 = new Storyboard();
			test2.Children.Add(testan2);
			Storyboard.SetTargetName(testan2, window.PlayedList.Name);
			Storyboard.SetTargetProperty(testan2, new PropertyPath(Grid.MarginProperty));
			test2.Begin(window.PlayedList);
		}
	}
}
