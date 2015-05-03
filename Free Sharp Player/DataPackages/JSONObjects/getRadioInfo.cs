﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Free_Sharp_Player {
	public class getRadioInfo : ViewModelNotifier {
		public bool up { get; set; }
		public String listeners { get { return GetProp<String>(); } set { SetProp(value); } }
		public String title { get { return GetProp<String>(); } set { SetProp(value); } }
		public String rating { get { return GetProp<String>(); } set { SetProp(value); } }
		public String url { get { return GetProp<String>(); } set { SetProp(value); } }
		public ServList servers;

		public class ServList {
			[JsonProperty("80 KBPS")]
			public String lowQuality { get; set; }
			[JsonProperty("128 KBPS")]
			public String medQuality { get; set; }
			[JsonProperty("320 KBPS")]
			public String highQuality { get; set; }
		}
		public String track_id { get { return GetProp<String>(); } set { SetProp(value); } }
		public String autoDJ { get { return GetProp<String>(); } set { SetProp(value); } }

		public static getRadioInfo doPost() {
			var payload = new Dictionary<String, Object> {
				{"action", "getRadioInfo"},
			};

			String result = HttpPostRequest.PostRequest(payload);
			getRadioInfo temp = JsonConvert.DeserializeObject(Util.StringToDict(result)["data"], typeof(getRadioInfo)) as getRadioInfo;

			return temp;
		}


		public void Print() {
			Util.PrintLine(ConsoleColor.Yellow, "--------------------");
			Util.PrintLine(ConsoleColor.White, "url", ": " + url);
			Util.PrintLine(ConsoleColor.Yellow, "--------------------");
		}

	}
	
}
