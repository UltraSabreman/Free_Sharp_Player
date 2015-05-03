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
		private static String Address = "http://api.canterlothill.com/v1/";
		private static Object Lock = new Object();

		static HttpPostRequest() {

		}

		public static String PostRequest(Dictionary<String, Object> Payload, String addr = null) {
			//lock (Lock) {
				StringBuilder PostData = new StringBuilder();

				foreach (String s in Payload.Keys) {
					PostData.Append(HttpUtility.UrlEncode(s)).Append("=").Append(Payload[s].ToString());
					if (Payload.Keys.Last() != s)
						PostData.Append("&");
				}

				byte[] data = Encoding.ASCII.GetBytes(PostData.ToString());

				HttpWebRequest httpWReq;

				if (addr == null) {
					httpWReq = (HttpWebRequest)WebRequest.Create(Address);
					httpWReq.Method = "POST";
					httpWReq.ContentType = "application/x-www-form-urlencoded";
					httpWReq.ContentLength = data.Length;

					using (Stream stream = httpWReq.GetRequestStream())
						stream.Write(data, 0, data.Length);

				} else {
					httpWReq = (HttpWebRequest)WebRequest.Create(addr);
					httpWReq.Method = "POST";
					httpWReq.ContentType = "application/x-www-form-urlencoded";
					httpWReq.ContentLength = data.Length;

					using (Stream stream = httpWReq.GetRequestStream())
						stream.Write(data, 0, data.Length);
				}

				return new StreamReader(((HttpWebResponse)httpWReq.GetResponse()).GetResponseStream()).ReadToEnd();
			//}
		}
		
	}
}
