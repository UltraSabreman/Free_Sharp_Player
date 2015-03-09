using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Free_Sharp_Player {
	class RadioInfo: JsonBase {
		public bool up { get; set; }
		public String listeners { get { return GetProp<String>(); } set { SetProp(value); } }
		public String title { get { return GetProp<String>(); } set { SetProp(value); } }
		public String rating { get { return GetProp<String>(); } set { SetProp(value); } }
		public String url { get { return GetProp<String>(); } set { SetProp(value); } }

		class ServList {
			[JsonProperty("80 KBPS")]
			public String lowQuality { get; set; }
			[JsonProperty("128 KBPS")]
			public String lowQuality { get; set; }
			[JsonProperty("320 KBPS")]
			public String lowQuality { get; set; }
		}
		public String track_id { get { return GetProp<String>(); } set { SetProp(value); } }
		public String autoDJ { get { return GetProp<String>(); } set { SetProp(value); } }
	}
}
