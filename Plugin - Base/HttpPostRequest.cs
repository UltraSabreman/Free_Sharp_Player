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

namespace Plugin_Base {
	public class HttpPostRequest {
		private static Object Lock = new Object();

		static HttpPostRequest() {

		}

		public static String PostRequest(Dictionary<String, String> Payload, String addr) {
			//lock (Lock) {
				StringBuilder PostData = new StringBuilder();

				foreach (String s in Payload.Keys) {
					PostData.Append(HttpUtility.UrlEncode(s)).Append("=").Append(Payload[s]);
					if (Payload.Keys.Last() != s)
						PostData.Append("&");
				}

				byte[] data = Encoding.ASCII.GetBytes(PostData.ToString());

				int tries = 0;
				while (tries < 3) {
					HttpWebRequest httpWReq;

                    httpWReq = (HttpWebRequest)WebRequest.Create(addr);
					httpWReq.Method = "POST";
					httpWReq.ContentType = "application/x-www-form-urlencoded";
					httpWReq.ContentLength = data.Length;

					using (Stream stream = httpWReq.GetRequestStream())
						stream.Write(data, 0, data.Length);
					
					try {
						return new StreamReader(((HttpWebResponse)httpWReq.GetResponse()).GetResponseStream()).ReadToEnd();
					} catch (Exception e) {
                        //Util.DumpException(e);
                    }
					tries++;
				}
				return null;
			//}
		}
		
	}
}
