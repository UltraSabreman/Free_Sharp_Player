using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Free_Sharp_Player {
	class getVoteStatus : ViewModelNotifier {
		int? vote; //my vote.
		int? status; //if here i can vote

		public static getVoteStatus doPost() {
			var payload = new Dictionary<String, Object> {
				{"action", "getVoteStatus"},
			};

			String result = HttpPostRequest.APICall(payload);
			getVoteStatus temp = JsonConvert.DeserializeObject(Util.StringToDict(result)["data"], typeof(getVoteStatus)) as getVoteStatus;

			return temp;
		}
	}
}
