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
		private static String ApiKey;

		public static List<String> GetAvalibleActions() {
			return new List<String>() {
				"client-ip",
				"debug",
				"playlist",
				"radio-info",
				"video-info",
				"queue",
				"playlist",
				"site-section"
			};
		}

		static HttpPostRequest() {
			var payload = new Dictionary<string,object>() {
				{ "action", "client-ip" },
			};
			var thing = APICall(payload);
			var temp = thing["data"] as Dictionary<String, Object>;
			Address = temp["IP"].ToString();

			//TODO: make this AES encryptind string stored in code for *release* only.
			using (StreamReader r = new StreamReader(File.OpenRead("Misc\\apikey.txt"))) {
				ApiKey = r.ReadLine();
			}
		}

		public static Dictionary<String, Object> APICall(Dictionary<String, Object> Payload) {
			StringBuilder PostData = new StringBuilder();
			String timeStamp = ((Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds).ToString();

			PostData.Append("&timestamp=").Append(timeStamp);

			StringBuilder payloadBuilder = new StringBuilder();
			foreach (String s in Payload.Keys)
				payloadBuilder.Append("payload%5b").Append(HttpUtility.UrlEncode(s)).Append("%5d=").Append(Payload[s].ToString());
			PostData.Append("&").Append(payloadBuilder.ToString());

			// JsonConvert.SerializeObject(new asdf() { action = "debug" }, Formatting.None);

			PostData.Append("&signature=");

			HttpWebRequest httpWReq = (HttpWebRequest)WebRequest.Create("http://everfree.net/api/");
			byte[] data = Encoding.ASCII.GetBytes(PostData.ToString());

			httpWReq.Method = "POST";
			httpWReq.ContentType = "application/x-www-form-urlencoded";
			httpWReq.ContentLength = data.Length;

			using (Stream stream = httpWReq.GetRequestStream())
				stream.Write(data, 0, data.Length);

			String response = new StreamReader(((HttpWebResponse)httpWReq.GetResponse()).GetResponseStream()).ReadToEnd();

			return ParseReturn(response);
		}
		public static Dictionary<String, Object> SecureAPICall(Dictionary<String, Object> Payload) {
			StringBuilder PostData = new StringBuilder();
			String timeStamp = ((Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds).ToString();

			PostData.Append("appID=").Append(Configs.Properties["AppID"].ToString());
			PostData.Append("&timestamp=").Append(timeStamp);

			StringBuilder payloadBuilder = new StringBuilder();
			StringBuilder payloadShaVersion = new StringBuilder();
			foreach (String s in Payload.Keys) {
				payloadBuilder.Append("payload%5b").Append(HttpUtility.UrlEncode(s)).Append("%5d=").Append(Payload[s].ToString()).Append("&");
				payloadShaVersion.Append(HttpUtility.UrlEncode(s)).Append("=").Append(Payload[s].ToString()).Append("&");
			}
			payloadBuilder.Remove(payloadBuilder.Length - 1, 1);
			payloadShaVersion.Remove(payloadShaVersion.Length - 1, 1);
			PostData.Append("&").Append(payloadBuilder.ToString());

			// JsonConvert.SerializeObject(new asdf() { action = "debug" }, Formatting.None);

			String plainSig = "ultrasaberman.freesharpplayer" + ApiKey + timeStamp + payloadShaVersion.ToString() + Address;
			Byte[] sigBytes = Encoding.ASCII.GetBytes(plainSig.ToCharArray());
			Byte[] shaBytes = SHA256.Create().ComputeHash(sigBytes);

			StringBuilder hex = new StringBuilder(shaBytes.Length * 2);
			foreach (byte b in shaBytes)
				hex.AppendFormat("{0:x2}", b);

			PostData.Append("&signature=").Append(hex.ToString());

			HttpWebRequest httpWReq = (HttpWebRequest)WebRequest.Create("http://everfree.net/api/");
			byte[] data = Encoding.ASCII.GetBytes(PostData.ToString());

			httpWReq.Method = "POST";
			httpWReq.ContentType = "application/x-www-form-urlencoded";
			httpWReq.ContentLength = data.Length;

			using (Stream stream = httpWReq.GetRequestStream())
				stream.Write(data, 0, data.Length);

			HttpWebResponse resp;
			try {
				resp = (HttpWebResponse)httpWReq.GetResponse();
			} catch (WebException e) {
				resp = (HttpWebResponse)e.Response;
			}

			String response = new StreamReader(resp.GetResponseStream()).ReadToEnd();

			return ParseReturn(response);
		}

		private static Dictionary<String, Object> ParseReturn(String msg) {
			var outd = new Dictionary<String, Object>();

			var test = JsonConvert.DeserializeObject(msg) as JObject;
			foreach (JProperty p in test.Properties()) {
				if (p.Value.GetType() == typeof(JObject))
					outd[p.Name] = ParseReturn(p.Value.ToString());
				else
					outd[p.Name] = p.Value.ToString();
			}

			return outd;
		}
	}
}
