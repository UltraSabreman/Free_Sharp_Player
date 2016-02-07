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

		public String WholeTitle { get { return (!String.IsNullOrEmpty(artist) ? artist.Trim() + " - " : "") + title.Trim(); } }


		public String duration { get; set; }
		public String plays { get; set; }

		public Track This { get { return this; } }

		public String rating { get { return GetProp<String>(); } set { SetProp(value); } }
		public String requests { get; set; }

		public int favorites { get { return GetProp<int>(); } set { SetProp(value); } }

		public String lastPlayed { get; set; } //THIS IS UTC TIME IF GOT FROM getTrack
		public DateTime localLastPlayed {
			get {
				return TimeZoneInfo.ConvertTimeFromUtc(DateTime.Parse(lastPlayed), TimeZoneInfo.Local);
			}
		}

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
			duration = src.duration;
			plays = src.plays;
			rating = src.rating;
			requests = src.requests;
			favorites = src.favorites;
			lastPlayed = src.lastPlayed;
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
