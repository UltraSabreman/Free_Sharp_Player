using System;

namespace Plugin {
    public class SetRequests {
		public int? submitted;
		public RequestError error;

	}

	public class RequestError {
		public int status;
		public int code;
		public String message;
	}

}

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
