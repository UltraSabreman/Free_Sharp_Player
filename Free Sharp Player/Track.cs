using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Free_Sharp_Player {
	class Track : PropChange {
		public int TrackID { get; set; }

		private String artist;
		public String Artist { get { return artist; } set { artist = value; OnPropertyChanged("Artist"); } }

		private String title;
		public String Title { get { return title; } set { title = value; OnPropertyChanged("Title"); } }

		public int Duration { get; set; }
		public int Plays { get; set; }

		private int rating;
		public int Rating { get { return rating; } set { rating = value; OnPropertyChanged("Rating"); } }
		public int Requests { get; set; }

		private int favorites;
		public int Favorites { get { return favorites; } set { favorites = value; OnPropertyChanged("Favorites"); } }

		public double LastPlayed { get; set; }
		public double DateAdded { get; set; }
		
		private int vote;
		public int Vote { get { return vote; } set { vote = value; OnPropertyChanged("Vote"); } }

		public double RequestTime { get; set; }
		public bool Forced { get; set; }
		public String Requester { get; set; }
		public bool Priority { get; set; }

		public static Track Parse(String s) {
			return JsonConvert.DeserializeObject(s) as Track;
		}

		public override string ToString() {
			return JsonConvert.SerializeObject(this, Formatting.None);
		}

	}
}
