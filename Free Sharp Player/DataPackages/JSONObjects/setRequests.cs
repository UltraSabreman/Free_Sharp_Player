using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Free_Sharp_Player {
	/*
{
    "action": "setRequest",
    "data": {
        "submitted": 1
    },
    "valid": 1
}
	 OR
{
    "action": "setRequest",
    "error": {
        "status": 2,
        "code": 500,
        "message": "Unable to request song"
    }
}
	 * 
	 * requester
	 * trackID

	 */
	public class setRequests : ViewModelNotifier {
		public int? submitted;

		public static setRequests doPost(String trackID) {
			var payload = new Dictionary<String, Object> {
				{"action", "setRequest"},
				{"trackID", trackID}
			};

			String result = HttpPostRequest.APICall(payload);
			setRequests temp = JsonConvert.DeserializeObject(Util.StringToDict(result)["data"], typeof(setRequests)) as setRequests;

			return temp;
		}
	}


}
