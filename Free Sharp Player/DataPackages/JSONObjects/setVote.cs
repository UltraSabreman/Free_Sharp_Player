using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Free_Sharp_Player {
	//Vote on spesific song.trackID
	class setVote : ViewModelNotifier {
		public int status;
		public int vote;
		public int inserted;
		public int updated;
		public int deleted;

		public static setVote doPost(int vote, String trackID = null) {
			var payload = new Dictionary<String, Object> {
				{"action", "setVote"},
			};
			if (trackID != null)
				payload["trackID"] = trackID;

			String result = HttpPostRequest.APICall(payload);
			setVote temp = JsonConvert.DeserializeObject(Util.StringToDict(result)["data"], typeof(setVote)) as setVote;

			return temp;
		}
	}
}
