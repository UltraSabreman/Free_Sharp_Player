using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using Plugin_Base;

namespace Free_Sharp_Player {
	public partial class BasicRequestUI : Window {
		public ObservableCollection<Track> TrackList { get; set; }

		public BasicRequestUI() {
			InitializeComponent();

			ResultsData.DataContext = this;

			TrackList = new ObservableCollection<Track>();
		}

        //TODO: integrate plugins
		private void DoSearch(object sender, TextChangedEventArgs e) {
			TextBox box = (sender as TextBox);
			if (box == null || box.Text == null || box.Text.Length < 3) return;
			String text = box.Text;

			/*getTrack get = null;
			if (box.Name == "NameBox")
				get = getTrack.doPost(null, null, text, null, null); //by Artist
			else
				get = getTrack.doPost(null, text, null, null, null); //by Title

			if (get == null || get.total_records == "0") return;

			TrackList.Clear();
			foreach (Track t in get.track)
				TrackList.Add(t);
                */
		}

		private void Button_Click(object sender, RoutedEventArgs e) {
			String trackID = (sender as Button).Tag as String;


			/*try {
				var ret = setRequests.doPost(trackID);
			} catch (Exception) { 
				var but = MessageBox.Show("The request erroed out.\n\nHit OK to go back, CANCEL to exit", "Request Error", MessageBoxButton.OKCancel, MessageBoxImage.Error, MessageBoxResult.OK);
				if (but == MessageBoxResult.OK) return;
			}*/
			this.Close();
		}

	}
}
