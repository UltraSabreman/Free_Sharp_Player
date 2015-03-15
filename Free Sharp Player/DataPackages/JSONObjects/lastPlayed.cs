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
            "last_played": "2015-03-14 18:04:19",
            "artist": "&I feat. Giggly Maria",
            "title": "Dive (Exiark's Endless Sky Remix)"
        },
        {
            "last_played": "2015-03-14 18:02:59",
            "artist": "Baq5",
            "title": "Egarim (Interlude) (ft.Rina-chan)"
        },
        {
            "last_played": "2015-03-14 17:56:07",
            "artist": "Ricardojvc6",
            "title": "Requiem Concerto Pour Octavia"
        },
        {
            "last_played": "2015-03-14 17:53:18",
            "artist": "Blaze",
            "title": "Love Is In Bloom"
        },
        {
            "last_played": "2015-03-14 17:47:11",
            "artist": "ArtAttack and Metajoker",
            "title": "Still Shy (Aviators Remix)"
        },
        {
            "last_played": "2015-03-14 17:43:49",
            "artist": "Daniel Ingram",
            "title": "Under Our Spell (174UDSI Mica Cover Remix)"
        },
        {
            "last_played": "2015-03-14 17:37:05",
            "artist": "Baasik",
            "title": "23"
        },
        {
            "last_played": "2015-03-14 17:31:04",
            "artist": "Sebastian Ingrosso & Tommy Trash Vs. Daniel Ingram",
            "title": "Reload the Crystal Empire (Nitramcz mashup)"
        },
        {
            "last_played": "2015-03-14 17:26:35",
            "artist": "Erutan",
            "title": "Children of the Night (Donn DeVore Symphonic Metal Cover)"
        },
        {
            "last_played": "2015-03-14 17:23:00",
            "artist": "Vocal Score",
            "title": "Under Luna's Sky"
        }
    ],
    "valid": 1
}
*/
	public class lastPlayed : ViewModelNotifier {
		public String last_played;
		public String artist;
		public String title;


		public static List<lastPlayed> doPost() {
			var payload = new Dictionary<String, Object> {
				{"action", "getLastPlayed"},
			};

			String result = HttpPostRequest.APICall(payload);
			List<lastPlayed> temp = JsonConvert.DeserializeObject(Util.StringToDict(result)["data"], typeof(List<lastPlayed>)) as List<lastPlayed>;

			return temp;
		}
	}
}
