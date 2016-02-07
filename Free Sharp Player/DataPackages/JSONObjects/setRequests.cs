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
	 * 
	    "error": {
        "status": 3,
        "code": 500,
        "message": "You must wait an hour in between requests"
    }
	 */
	public class setRequests : ViewModelNotifier {
		public int? submitted;
		public RequestError error;
		//TODO: hanlde errors (Add error checking return)

		public static setRequests doPost(String trackID) {
			var payload = new Dictionary<String, Object> {
				{"action", "setRequest"},
				{"trackID", Uri.EscapeUriString(trackID)}
			};

			String result = HttpPostRequest.PostRequest(payload);
			setRequests temp = JsonConvert.DeserializeObject(Util.StringToDict(result)["data"], typeof(setRequests)) as setRequests;

			return temp;
		}
	}

	public class RequestError {
		public int status;
		public int code;
		public String message;
	}

}
