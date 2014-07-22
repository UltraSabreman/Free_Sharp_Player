using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Free_Sharp_Player {
	class PlaylistViewModel : PropChange {
		private List<Track> played = new List<Track>();
		public List<Track> Played { get { return played; } set { played = value; OnPropertyChanged("Played"); } }

		private List<Track> queue = new List<Track>();
		public List<Track> Queue { get { return queue; } set { queue = value; OnPropertyChanged("Queue"); } }

		private Track playing;
		public Track Playing { get { return playing; } set { playing = value; OnPropertyChanged("Playing"); } }
	}
}
