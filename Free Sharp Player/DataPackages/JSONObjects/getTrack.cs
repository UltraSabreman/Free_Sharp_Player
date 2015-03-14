using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Free_Sharp_Player {
	public class getTrack : JSONBase {
		public Data data;

		public class Data : ViewModelNotifier {
			public String total_records;
			public int pages;
			public List<Track> track;
			public Vars variables;

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

				public String LastPlayed { get; set; }
				public double DateAdded { get; set; }

				public int requestable { get { return GetProp<int>(); } set { SetProp(value); } }

				public double RequestTime { get; set; }
				public int forced { get { return GetProp<int>(); } set { SetProp(value); } }
				public String Requester { get { return GetProp<String>(); } set { SetProp(value); } }
				public int Priority { get; set; }

				public static Track Parse(String s) {
					return JsonConvert.DeserializeObject(s) as Track;
				}

				public override string ToString() {
					return JsonConvert.SerializeObject(this, Formatting.None);
				}
			}

			public class Vars {
				public String artist;
				public String Track;
				public int page;
				public int limit;
				public String rating;
				public String rating_direction;
			}
		}

	}
}
