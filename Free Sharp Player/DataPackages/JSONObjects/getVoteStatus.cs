using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Free_Sharp_Player {
	class getVoteStatus : ViewModelNotifier {
		public int? vote; //my vote.
		public int? status; //if here i can vote

		public static getVoteStatus doPost(String trackid = null) {
			var payload = new Dictionary<String, Object> {
				{"action", "getVoteStatus"},
			};

			if (trackid != null) payload["trackID"] = Uri.EscapeUriString(trackid);

			String result = HttpPostRequest.PostRequest(payload);
			getVoteStatus temp = JsonConvert.DeserializeObject(Util.StringToDict(result)["data"], typeof(getVoteStatus)) as getVoteStatus;

			return temp;
		}
	}
}
