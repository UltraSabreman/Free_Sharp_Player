using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Free_Sharp_Player {
	public class Track : ViewModelNotifier {
		public String trackID { get; set; }

		public String artist { get { return GetProp<String>(); } set { SetProp(value + " "); } }

		public String title { get { return GetProp<String>(); } set { SetProp(value); } }

		public String WholeTitle { get { return artist + " - " + title; } set { } }


		public String duration { get; set; }
		public String plays { get; set; }

		public String rating { get { return GetProp<String>(); } set { SetProp(value); } }
		public String requests { get; set; }

		public int favorites { get { return GetProp<int>(); } set { SetProp(value); } }

		public String lastPlayed { get; set; }
		public DateTime localLastPlayed { get; set; }

		public int requestable { get { return GetProp<int>(); } set { SetProp(value); } }

		public double RequestTime { get; set; }
		public int forced { get { return GetProp<int>(); } set { SetProp(value); } }
		public String Requester { get { return GetProp<String>(); } set { SetProp(value); } }
		public int Priority { get; set; }

		public int MyVote { get; set; }

		public static Track Parse(String s) {
			return JsonConvert.DeserializeObject(s) as Track;
		}

		public override string ToString() {
			return JsonConvert.SerializeObject(this, Formatting.None);
		}

		public void Update(Track src) {
			trackID = src.trackID;
			artist = src.artist;
			title = src.title;
			WholeTitle = src.WholeTitle;
			duration = src.duration;
			plays = src.plays;
			rating = src.rating;
			requests = src.requests;
			favorites = src.favorites;
			lastPlayed = src.lastPlayed;
			localLastPlayed = src.localLastPlayed;
			requestable = src.requestable;
			RequestTime = src.RequestTime;
			forced = src.forced;
			Requester = src.Requester;
			Priority = src.Priority;
			MyVote = src.MyVote;
		}

		public void Print() {
			Util.PrintLine(ConsoleColor.Yellow, "--------------------");
			Util.PrintLine(ConsoleColor.White, "TrackID", ": " + trackID);
			Util.PrintLine(ConsoleColor.White, "WholeTitle", ": " + WholeTitle);
			Util.PrintLine(ConsoleColor.White, "Duration", ": " + duration);
			Util.PrintLine(ConsoleColor.White, "lastPlayed", ": " + lastPlayed);
			Util.PrintLine(ConsoleColor.Yellow, "--------------------");
		}

		public String getLastPlayed() {
			return DateTime.Parse(lastPlayed).ToString("mm:ss");
		}
	}
}
