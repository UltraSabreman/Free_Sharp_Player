using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Free_Sharp_Player {
	/*
{
    "action": "getLastPlayed",
    "data": [
        {
            "last_played": "2015-05-03 13:17:59",
            "artist": "Daniel Ingram",
            "title": "Find A Way (Azuredash Remix)",
            "trackID": "13137"
        },
        {
            "last_played": "2015-05-03 13:13:39",
            "artist": "Flying Melodys",
            "title": "Let Your Heart Be Free",
            "trackID": "17543"
        },
        {
            "last_played": "2015-05-03 13:11:40",
            "artist": "Daniel Ingram",
            "title": "Life is a Runway",
            "trackID": "18105"
        },
        {
            "last_played": "2015-05-03 13:08:39",
            "artist": "Resonantwaves",
            "title": "Beneath the Trenches",
            "trackID": "8373"
        },
        {
            "last_played": "2015-05-03 13:05:24",
            "artist": "H8_Seed and Wooden Toaster",
            "title": "Awoken (Vocal Score Cover)",
            "trackID": "13325"
        },
        {
            "last_played": "2015-05-03 13:01:40",
            "artist": "Psycosis",
            "title": "Smile 3x",
            "trackID": "1527"
        },
        {
            "last_played": "2015-05-03 12:57:18",
            "artist": "Luna Jax",
            "title": "The Choice I Have Made",
            "trackID": "17008"
        },
        {
            "last_played": "2015-05-03 12:53:48",
            "artist": "Flying Melodys",
            "title": "Just Keep Smiling",
            "trackID": "16939"
        },
        {
            "last_played": "2015-05-03 12:50:38",
            "artist": "WoodenToaster",
            "title": "Rainbow Factory (DJDoctorWhooves Guitar Cover)",
            "trackID": "14604"
        },
        {
            "last_played": "2015-05-03 12:47:14",
            "artist": "Kaoss Walker",
            "title": "Crystal Heart Return",
            "trackID": "15778"
        }
    ],
    "valid": 1
}
*/
	public class lastPlayed : ViewModelNotifier {
		public String last_played;
		public String artist;
		public String title;
		public String trackID;


		public static List<lastPlayed> doPost() {
			var payload = new Dictionary<String, Object> {
				{"action", "getLastPlayed"},
			};

			String result = HttpPostRequest.PostRequest(payload);
			List<lastPlayed> temp = JsonConvert.DeserializeObject(Util.StringToDict(result)["data"], typeof(List<lastPlayed>)) as List<lastPlayed>;

			return temp;
		}
	}
}
