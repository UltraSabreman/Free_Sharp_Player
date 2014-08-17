using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Free_Sharp_Player {
	class PlaylistModel : ViewModelNotifier {
		public ObservableCollection<Track> Played { get { return GetProp<ObservableCollection<Track>>(); } set { SetProp(value); } }
		public ObservableCollection<Track> Queue { get { return GetProp<ObservableCollection<Track>>(); } set { SetProp(value); } }

		public Track playing;
		public ObservableCollection<Track> Playing { get { return new ObservableCollection<Track> { playing }; } set { playing = value.First(); OnPropertyChanged("Playing"); } }
	}
}
