using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace Free_Sharp_Player {
	//TODO: sanity check duration value
	class PlaylistModel : ViewModelNotifier {
		public ObservableCollection<Track> Played { get { return GetProp<ObservableCollection<Track>>(); } set { SetProp(value); } }
		public ObservableCollection<Track> Queue { get { return GetProp<ObservableCollection<Track>>(); } set { SetProp(value); } }

		MainWindow window;
		private bool isOpen = false;

		public PlaylistModel(MainWindow win) {
			SetWindow(win);
		}

		public PlaylistModel() {

		}

		public void SetWindow(MainWindow win) {
			window = win;

			window.QueueList.DataContext = this;
			window.PlayedList.DataContext = this;
			UpdateSize();

			window.QueueList.Margin = new Thickness(0, window.QueueHeight.Height.Value, 0, 0);
			window.PlayedList.Margin = new Thickness(0, 0, 0, window.PlayedHeight.Height.Value);
		}

		public void UpdateSize() {
			double Size =(Queue.Count * 20);
			window.QueueHeight.Height = new System.Windows.GridLength(Size);
			
			Size = (Played.Count * 20);
			window.PlayedHeight.Height = new System.Windows.GridLength(Size);
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
