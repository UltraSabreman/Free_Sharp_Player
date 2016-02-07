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
	
		public String StreamTitle { get { return GetProp<String>(); } set { SetProp(value); DoMarquee(); } }
		public ObservableCollection<lastPlayed> Played { get { return GetProp<ObservableCollection<lastPlayed>>(); } set { SetProp(value); } }
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
			Played = new ObservableCollection<lastPlayed>();
			Queue = new ObservableCollection<Track>();

			SetWindow(win);
		}

		public void SetWindow(MainWindow win) {
			window = win;

			window.QueueList.DataContext = this;
			window.PlayedList.DataContext = this;
			window.txt_TrackName.DataContext = this;


			Tick(null, null);

			//window.QueueList.Margin = new Thickness(0, window.QueueHeight.Height.Value, 0, 0);
			//window.PlayedList.Margin = new Thickness(0, 0, 0, window.PlayedHeight.Height.Value);
			MaxQueueHight = MinQueueHeight = 0;// window.QueueHeight.Height.Value;
			MaxPlayedHight = MinPlayedHeight = 0;// window.PlayedHeight.Height.Value;


			window.LocationChanged += (o, e) => {
				window.Played.HorizontalOffset += 1;
				window.Played.HorizontalOffset -= 1;

				window.Queue.HorizontalOffset += 1;
				window.Queue.HorizontalOffset -= 1;
			};

			window.Deactivated += (o, e) => {
				window.Queue.IsOpen = false;
				window.Played.IsOpen = false;
				doOpen = false;
			};

			listUpdater.Elapsed += Tick;
			listUpdater.AutoReset = true;
			listUpdater.Start();

			StreamTitle = "Not Connected";

			//DoMarquee();
		}

		public void Tick(Object o, EventArgs e) {
            List<lastPlayed> tempPlayed = lastPlayed.doPost();
            getRequests tempQueue = getRequests.doPost();

            window.Dispatcher.Invoke(new Action(() => {
                lock (theLock) {
                    Played.Clear();
                    if (tempPlayed != null)
                        tempPlayed.ForEach(Played.Add);

                    Queue.Clear();
                    if (tempQueue != null && tempQueue.track != null)
                        tempQueue.track.ForEach(Queue.Add);

                    UpdateSize();
                    //window.PlayedList.Margin = new Thickness(0, MinPlayedHeight, 0, 0);
                    //window.QueueList.Margin = new Thickness(0, 0, 0, MinQueueHeight);
                }
            }));

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
					DoubleAnimation anim2 = new DoubleAnimation((text.ActualWidth + canvas.ActualWidth / 2), 0, new Duration(new TimeSpan(0, 0, 10)));
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
				MaxQueueHight = (Queue.Count * 15);
			else
				MaxQueueHight = 50;

			if (Played != null && Played.Count > 0)
				MaxPlayedHight = (Played.Count * 15);
			else
				MaxPlayedHight = 0;

			window.Dispatcher.Invoke(new Action(() => {
				window.Played.Height = MaxPlayedHight;
				window.Queue.Height = MaxQueueHight;
				//window.Queue.VerticalOffset = - (MaxQueueHight);
				//App.Current.MainWindow.Height = MaxPlayedHight + MaxQueueHight + 30;
			}));
		}

		public void AnimateLists() {
			doOpen = !doOpen;
			window.Played.IsOpen = doOpen;
			window.Queue.IsOpen = doOpen;
			/*if (doOpen) {
				window.btn_PlayPause.Background = Brushes.Red;
				window.Played.IsOpen = true;
			} else {
				window.btn_PlayPause.Background = Brushes.Green;
				window.Played.IsOpen = false;
			}*/
		

			/*DoubleAnimation testan;
			if (doOpen) {
				testan = new DoubleAnimation(0, MaxQueueHight, new Duration(new TimeSpan(0, 0, 0, 0, 100)), FillBehavior.HoldEnd);
			} else {
				testan = new DoubleAnimation(MaxQueueHight, 0, new Duration(new TimeSpan(0, 0, 0, 0, 100)), FillBehavior.HoldEnd);
			}

			Storyboard test = new Storyboard();
			test.Children.Add(testan);
			Storyboard.SetTargetName(testan, window.QueueList.Name);
			Storyboard.SetTargetProperty(testan, new PropertyPath(ListView.HeightProperty));
			test.Begin(window.QueueList);

			DoubleAnimation testan2;
			if (doOpen) {
				testan2 = new DoubleAnimation(0, MaxPlayedHight, new Duration(new TimeSpan(0, 0, 0, 0, 100)), FillBehavior.HoldEnd);
			} else {
				testan2 = new DoubleAnimation(MaxPlayedHight, 0, new Duration(new TimeSpan(0, 0, 0, 0, 100)), FillBehavior.HoldEnd);
			}

			Storyboard test2 = new Storyboard();
			test2.Children.Add(testan2);
			Storyboard.SetTargetName(testan2, window.PlayedList.Name);
			Storyboard.SetTargetProperty(testan2, new PropertyPath(ListView.HeightProperty));
			test2.Begin(window.PlayedList);
			if (doOpen)
				Util.AnimateWindowMoveY(window, MaxQueueHight);
			else
				Util.AnimateWindowMoveY(window, -(MaxQueueHight));*/
		}


	}
}
