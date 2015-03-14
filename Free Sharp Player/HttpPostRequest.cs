using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Web;
using System.Net;
using System.IO;

namespace Free_Sharp_Player {
	class HttpPostRequest {
		private static String Address ="";

		public static List<String> GetAvalibleActions() {
			return new List<String>() {
				"getRadioInfo",
				"getTrack",
				"getVoteStatus",
				"setVote",
			};
		}

		class TestIP {
			public String IP = "";
		}

		static HttpPostRequest() {
			/*var payload = new Dictionary<string,object>() {
				{ "action", "client-ip" },
			};
			var thing = APICall(payload);
			String temp = thing["data"].ToString();
			//var temp = thing["data"] as Dictionary<String, Object>;
			//Address = temp["IP"].ToString();

			Address = (JsonConvert.DeserializeObject(temp, typeof(TestIP)) as TestIP).IP;*/

			//TODO: make this AES encryptind string stored in code for *release* only.
			/*using (StreamReader r = new StreamReader(File.OpenRead("Misc\\apikey.txt"))) {
				ApiKey = r.ReadLine();
			}*/
		}

		public static String APICall(Dictionary<String, Object> Payload) {
			StringBuilder PostData = new StringBuilder();

			foreach (String s in Payload.Keys) {
				PostData.Append(HttpUtility.UrlEncode(s)).Append("=").Append(Payload[s].ToString());
				if (Payload.Keys.Last() != s)
					PostData.Append("&");
			}

			HttpWebRequest httpWReq = (HttpWebRequest)WebRequest.Create("http://canterlothill.com/api/v1/");
			byte[] data = Encoding.ASCII.GetBytes(PostData.ToString());

			httpWReq.Method = "POST";
			httpWReq.ContentType = "application/x-www-form-urlencoded";
			httpWReq.ContentLength = data.Length;

			using (Stream stream = httpWReq.GetRequestStream())
				stream.Write(data, 0, data.Length);

			String response = new StreamReader(((HttpWebResponse)httpWReq.GetResponse()).GetResponseStream()).ReadToEnd();

			return response;
		}
		
	}
}
