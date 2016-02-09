using Newtonsoft.Json;
using Plugin_Base.DataPackages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin_Canterlot_Hill {
	public class SetRequests : BaseDataPackage {
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
