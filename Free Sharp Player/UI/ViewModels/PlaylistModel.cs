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
	//TODO: sanity check duration value
	//tODO: make icons for times show up on main bar when epxaneded? (or not
	//Tool tips on time and stuff
	//make extra menu apear on right click of anything.
	//replace extra icon with app icon?
	//make marqueu take only 50%?
	//how to handle requester?
	class PlaylistModel : ViewModelNotifier {
		public ObservableCollection<Track> Played { get { return GetProp<ObservableCollection<Track>>(); } set { SetProp(value); } }
		public ObservableCollection<Track> Queue { get { return GetProp<ObservableCollection<Track>>(); } set { SetProp(value); } }

		MainWindow window;
		private bool isOpen = false;
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

			Tick(null, null);

			window.QueueList.Margin = new Thickness(0, window.QueueHeight.Height.Value, 0, 0);
			window.PlayedList.Margin = new Thickness(0, 0, 0, window.PlayedHeight.Height.Value);


			listUpdater.Elapsed += Tick;
			listUpdater.AutoReset = true;
			listUpdater.Start();

		}

		public void Tick(Object o, EventArgs e) {
			var payload = new Dictionary<string, object>() {
				{ "action", "playlist" },
			};

			String playListData = HttpPostRequest.SecureAPICall(payload)["data"].ToString();
			Util.Print(playListData);


			PlaylistModel temp = (JsonConvert.DeserializeObject(playListData, typeof(PlaylistModel)) as PlaylistModel);
			Played = temp.Played;
			Queue = temp.Queue;

			UpdateSize();
		}

		public void UpdateSize() {
			double Size =(Queue.Count * 20);
			window.Dispatcher.Invoke(new Action(() => {
				window.QueueHeight.Height = new System.Windows.GridLength(Size);

				Size = (Played.Count * 20);
				window.PlayedHeight.Height = new System.Windows.GridLength(Size);
			}));
		}

		public void AnimateLists() {
			isOpen = !isOpen;
			ThicknessAnimation testan;
			if (isOpen) {
				testan = new ThicknessAnimation(new Thickness(0, window.QueueHeight.Height.Value, 0, 0), new Thickness(0, 0, 0, 0), new Duration(new TimeSpan(0, 0, 0, 0, 100)), FillBehavior.HoldEnd);
			} else {
				testan = new ThicknessAnimation(new Thickness(0, 0, 0, 0), new Thickness(0, window.QueueHeight.Height.Value, 0, 0), new Duration(new TimeSpan(0, 0, 0, 0, 100)), FillBehavior.HoldEnd);
			}

			ThicknessAnimation testan2;
			if (isOpen) {
				testan2 = new ThicknessAnimation(new Thickness(0, 0, 0, window.PlayedHeight.Height.Value), new Thickness(0, 0, 0, 0), new Duration(new TimeSpan(0, 0, 0, 0, 100)), FillBehavior.HoldEnd);
			} else {
				testan2 = new ThicknessAnimation(new Thickness(0, 0, 0, 0), new Thickness(0, 0, 0, window.PlayedHeight.Height.Value), new Duration(new TimeSpan(0, 0, 0, 0, 100)), FillBehavior.HoldEnd);
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
