using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Web;
using System.Net;
using System.IO;

namespace Free_Sharp_Player {
	class HttpPostRequest {
		private static String Address = "http://canterlothill.com/api/v1/";

		public static String APICall(Dictionary<String, Object> Payload) {
			StringBuilder PostData = new StringBuilder();

			foreach (String s in Payload.Keys) {
				PostData.Append(HttpUtility.UrlEncode(s)).Append("=").Append(Payload[s].ToString());
				if (Payload.Keys.Last() != s)
					PostData.Append("&");
			}

			HttpWebRequest httpWReq = (HttpWebRequest)WebRequest.Create(Address);
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
