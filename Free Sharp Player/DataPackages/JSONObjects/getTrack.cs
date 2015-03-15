using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Free_Sharp_Player {
	public class getTrack : ViewModelNotifier {
		public String total_records;
		public int pages;
		public List<Track> track;
		public Vars variables;

		public class Vars {
			public String artist;
			public String Track;
			public int page;
			public int limit;
			public String rating;
			public String rating_direction;
		}

		public static getTrack doPost(String track, String artist) {
			return doPost(track, artist, null, null);
		}

		public static getTrack doPost(int? rating, String ratingEq) {
			return doPost(null, null, rating, ratingEq);
		}

		public static getTrack doPost(String track, String artist, int? rating, String ratingEq, int page = 1, int limit = 20) {
			var payload = new Dictionary<String, Object> {
				{"action", "getTrack"},
				{"page", Uri.EscapeUriString(page.ToString())},
				{"limit", Uri.EscapeUriString(limit.ToString())},
			};

			if (track == null && artist == null && rating == null)
				throw new ArgumentNullException("Must provide either artist, rating, or track");

			if (track != null) payload["track"] = Uri.EscapeUriString(track);
			if (artist != null) payload["artist"] = Uri.EscapeUriString(artist);
			if (rating != null) {
				payload["rating"] = Uri.EscapeUriString(rating.ToString());
				if (ratingEq == null) throw new ArgumentNullException("Must provide ratingEq if spesifing rating.");
				payload["rating_inequality"] = Uri.EscapeUriString(ratingEq);
			}

			String result = HttpPostRequest.APICall(payload);
			getTrack temp = JsonConvert.DeserializeObject(Util.StringToDict(result)["data"], typeof(getTrack)) as getTrack;

			return temp;
		}	

	}
}
