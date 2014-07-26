using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Free_Sharp_Player {
	class Track : ViewModelNotifier {
		public int TrackID { get; set; }

		public String artist { get { return GetProp<String>(); } set { SetProp(value); } }

		public String title { get { return GetProp<String>(); } set { SetProp(value); } }

		public int Duration { get; set; }
		public int Plays { get; set; }

		public int rating { get { return GetProp<int>(); } set { SetProp(value); } }
		public int Requests { get; set; }

		public int favorites { get { return GetProp<int>(); } set { SetProp(value); } }

		public double LastPlayed { get; set; }
		public double DateAdded { get; set; }
		
		public int vote { get { return GetProp<int>(); } set { SetProp(value); } }

		public double RequestTime { get; set; }
		public int Forced { get; set; }
		public String Requester { get; set; }
		public int Priority { get; set; }

		public static Track Parse(String s) {
			return JsonConvert.DeserializeObject(s) as Track;
		}

		public override string ToString() {
			return JsonConvert.SerializeObject(this, Formatting.None);
		}

	}
}
