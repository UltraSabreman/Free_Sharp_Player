using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Free_Sharp_Player {
	/*
{
    "action": "getRequest",
    "data": {
        "track": [
            {
                "trackID": "6996",
                "artist": "Jeff Burgess",
                "title": "Trixie (Remaster)",
                "plays": "90",
                "requests": "51",
                "duration": "00:02:38",
                "lastPlayed": "2015-03-03 18:06:49",
                "rating": "299",
                "requester": "EarthenForge"
            },
            {
                "trackID": "16333",
                "artist": "SlyphStorm",
                "title": "Soldiers of the Night (ft. 4EverfreeBrony & Midnight Melody)",
                "plays": "228",
                "requests": "199",
                "duration": "00:06:27",
                "lastPlayed": "2015-03-13 16:54:00",
                "rating": "1049",
                "requester": ""
            }
        ],
        "number_of_tracks": 2
    },
    "valid": 1
}
	 */
	public class getRequests : ViewModelNotifier {
		public List<Track> track;
		public int number_of_tracks;

		public static getRequests doPost() {
			var payload = new Dictionary<String, Object> {
				{"action", "getRequest"},
			};

			String result = HttpPostRequest.PostRequest(payload);
			getRequests temp = JsonConvert.DeserializeObject(Util.StringToDict(result)["data"], typeof(getRequests)) as getRequests;

			return temp;
		}
	}
}


